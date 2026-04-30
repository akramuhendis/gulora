using AutoMapper;
using Gulora.Application.Common.Constants;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Orders;
using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.ValueObjects;
using MediatR;

namespace Gulora.Application.Orders.Commands.CreateOrder;

/// <summary>
/// <see cref="CreateOrderCommand"/> için MediatR işleyicisi. Ürünleri toplu yükler, satış ve stok
/// kurallarını uygular, teslimat ücreti ve kupon indirimini hesaplar, domain siparişini oluşturur,
/// stok düşümü ve kupon kullanım sayacını günceller ve değişiklikleri tek bir birim iş içinde kalıcı yazar.
/// </summary>
/// <remarks>
/// İş akışı sırası önemlidir: önce salt okuma ve doğrulama, ardından domain oluşturma, en son stok ve
/// kupon yan etkileri ile kalıcılık; böylece geçersiz siparişlerde stok düşülmez.
/// </remarks>
/// <param name="orders">Sipariş ekleme ve birim iş kaydı için sipariş deposu.</param>
/// <param name="products">Ürün toplu okuma ve stok güncellemesi için ürün deposu.</param>
/// <param name="coupons">Kupon kodu ile arama, indirim hesaplama ve kullanım sayacı için kupon deposu.</param>
/// <param name="mapper">
/// Oluşturulan <see cref="Order"/> varlığını API sözleşmesi <see cref="OrderDto"/> tipine dönüştürmek için kullanılır.
/// </param>
public sealed class CreateOrderCommandHandler(
    IOrderRepository orders,
    IProductRepository products,
    ICouponRepository coupons,
    IMapper mapper) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    /// <summary>
    /// Komut yükünü işler: ürün ve stok doğrulaması, fiyatlandırma, kupon, adres, sipariş oluşturma,
    /// stok düşümü, kupon kullanımı ve kalıcılık adımlarını sırayla yürütür.
    /// </summary>
    /// <param name="request">Oluşturulacak siparişin tüm girdi alanlarını içeren komut örneği.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Kalıcı kaydedilen siparişin <see cref="OrderDto"/> temsilini döndürür.</returns>
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: İstekteki tüm benzersiz ürün kimliklerini topla ve tek veritabanı sorgusu ile yükle.
        // Eksik kimlik varsa (istenen sayı ile dönen satır sayısı uyuşmuyorsa) anlamlı bir bulunamadı hatası fırlatılır.
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var loaded = await products.GetByIdsAsync(productIds, cancellationToken).ConfigureAwait(false);
        if (loaded.Count != productIds.Count)
            throw new NotFoundException("Product", string.Join(',', productIds));

        // Adım 2: Ürünleri kimliğe göre sözlükte tut; aşağıdaki kalemler ve teslimat kontrollerinde O(1) erişim sağlanır.
        var byId = loaded.ToDictionary(p => p.Id);
        // Her yüklenen ürün satışa açık olmalı; pasif ürünler erken reddedilir (domain istisnası).
        foreach (var p in loaded)
        {
            if (!p.IsActive)
                throw new DomainException($"Product '{p.Name}' is not available for sale.");
        }

        // Adım 3: Aynı gün teslimat seçildiyse, sepetteki her ürünün aynı gün uygunluğu zorunludur.
        if (request.DeliveryType == DeliveryType.SameDay)
        {
            foreach (var item in request.Items)
            {
                if (!byId[item.ProductId].IsSameDay)
                    throw new DomainException("At least one selected product is not eligible for same-day delivery.");
            }
        }

        // Adım 4: Her sipariş kalemi için stok yeterliliğini doğrula ve domain sipariş satırı listesini oluştur.
        var lines = new List<OrderLine>();
        foreach (var item in request.Items)
        {
            var p = byId[item.ProductId];
            if (p.Stock < item.Quantity)
                throw new DomainException($"Insufficient stock for product '{p.Name}'.");

            lines.Add(new OrderLine(
                p.Id,
                p.Name,
                p.ImageUrl,
                item.Quantity,
                p.Price,
                item.VariantLabel));
        }

        // Adım 5: Ara toplamı satır birim fiyat × miktar ile hesapla; tüm kalemler ücretsiz teslimata uygunsa
        // teslimat ücreti sıfırlanır, aksi halde uygulama sabitindeki standart ücret uygulanır.
        var subtotal = lines.Sum(l => l.UnitPrice * l.Quantity);
        var allLinesFreeDelivery = request.Items.All(i => byId[i.ProductId].IsFreeDelivery);
        var deliveryFee = allLinesFreeDelivery ? 0m : OrderPricing.StandardDeliveryFeeAzn;

        // Adım 6: Kupon kodu varsa: kodu normalize et, kuponu yükle, geçerliliği sipariş tutarına göre kontrol et,
        // indirimi hesapla ve toplam indirimin ara toplam + teslimat ücretini aşmamasını garanti et.
        decimal discountAmount = 0;
        Coupon? couponEntity = null;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var code = request.CouponCode.Trim().ToUpperInvariant();
            couponEntity = await coupons.GetByCodeAsync(code, cancellationToken).ConfigureAwait(false);
            if (couponEntity is null)
                throw new NotFoundException("Coupon", code);
            if (!couponEntity.IsValid(subtotal))
                throw new DomainException("The coupon is not valid for this order.");
            discountAmount = couponEntity.CalculateDiscount(subtotal);
            discountAmount = Math.Min(discountAmount, subtotal + deliveryFee);
        }

        // Adım 7: İstemci adres DTO’sundan değer nesnesi oluştur: metin alanları kırpılır, posta kodu boşsa null atanır.
        var address = new AddressSnapshot(
            request.Address.RecipientName.Trim(),
            request.Address.RecipientPhone.Trim(),
            request.Address.City.Trim(),
            request.Address.District.Trim(),
            request.Address.Line.Trim(),
            string.IsNullOrWhiteSpace(request.Address.PostalCode) ? null : request.Address.PostalCode.Trim());

        // Adım 8: Toplanan satırlar, adres, teslimat/ödeme, ücretler ve kupon bilgisi ile domain sipariş fabrikası çağrılır.
        var order = Order.Create(
            request.UserId,
            lines,
            address,
            request.DeliveryType,
            request.PaymentMethod,
            deliveryFee,
            discountAmount,
            string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            request.ScheduledDate);

        // Adım 9: Onaylanan her kalem için ilgili ürün varlığında stok düşümü yapılır (bellek içi, henüz SaveChanges öncesi).
        foreach (var item in request.Items)
        {
            var p = byId[item.ProductId];
            p.DecreaseStock(item.Quantity);
        }

        // Adım 10: Geçerli bir kupon kullanıldıysa kullanım sayacı artırılır (kampanya limitleri için).
        couponEntity?.IncrementUsage();

        // Adım 11: Yeni siparişi depoya ekle ve tüm takip eden değişiklikleri (stok, kupon, sipariş) tek seferde kaydet.
        await orders.AddAsync(order, cancellationToken).ConfigureAwait(false);
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Adım 12: Kalıcı durumu istemciye uygun DTO’ya haritala ve döndür.
        return mapper.Map<OrderDto>(order);
    }
}
