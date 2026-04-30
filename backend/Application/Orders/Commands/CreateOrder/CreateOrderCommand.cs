using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Orders.Commands.CreateOrder;

/// <summary>
/// Yeni bir sipariş oluşturma komutu. İstemciden gelen sepet kalemleri, teslimat adresi, teslimat türü,
/// ödeme yöntemi ve isteğe bağlı kupon ile not bilgilerini taşır; işleyici tarafında ürün yükleme,
/// stok ve fiyatlandırma, kupon ve adres doğrulaması sonrası domain <c>Order</c> varlığı oluşturulur
/// ve kalıcı olarak kaydedilir. Yanıt olarak tam sipariş görünümü <see cref="OrderDto"/> döner.
/// </summary>
/// <remarks>
/// Bu kayıt <see cref="ICommand"/> ile işaretlenir; uygulama katmanında komut/sorgu ayrımı ve
/// (varsa) birim iş, doğrulama ve yetkilendirme pipeline davranışları bu soyutlama üzerinden tetiklenir.
/// </remarks>
/// <param name="UserId">
/// Siparişi oluşturan son kullanıcının benzersiz kimliği; sipariş satırı ve faturalama bağlamında
/// sipariş sahibi olarak kalıcı kayda yazılır.
/// </param>
/// <param name="Items">
/// Sepetten gelen, ürün kimliği ve miktar (ve isteğe bağlı varyant etiketi) içeren sipariş kalemleri.
/// Aynı ürünün birden fazla ayrı satır olarak gelmesi doğrulayıcı tarafından reddedilir.
/// </param>
/// <param name="Address">
/// Teslimat için kullanılacak alıcı adı, telefon, şehir, ilçe, adres satırı ve isteğe bağlı posta kodu;
/// işleyicide <c>AddressSnapshot</c> değer nesnesine dönüştürülerek siparişe gömülür.
/// </param>
/// <param name="DeliveryType">
/// Teslimat modu (ör. aynı gün, planlı); aynı gün seçildiğinde her ürünün <c>IsSameDay</c> uygunluğu
/// iş kurallarıyla denetlenir; planlı modda <paramref name="ScheduledDate"/> zorunludur.
/// </param>
/// <param name="PaymentMethod">
/// Sipariş anında seçilen ödeme yöntemi; sipariş özetinde ve sonraki süreçlerde referans için saklanır.
/// </param>
/// <param name="CouponCode">
/// İsteğe bağlı kupon kodu; dolu ise büyük harfe normalize edilerek kupon deposundan yüklenir,
/// geçerlilik ve indirim tutarı hesaplanır. Boş veya yalnızca boşluk ise indirim uygulanmaz.
/// </param>
/// <param name="Notes">
/// Müşteri veya operasyon için serbest metin not; boşsa kayda yazılmaz, doluysa kırpılarak saklanır.
/// </param>
/// <param name="ScheduledDate">
/// Planlı teslimat türünde anlamlı olan hedef teslim tarihi/saati; diğer teslimat türlerinde
/// genellikle kullanılmaz veya domain tarafında yok sayılabilir.
/// </param>
public sealed record CreateOrderCommand(
    Guid UserId,
    IReadOnlyList<OrderItemRequest> Items,
    AddressRequest Address,
    DeliveryType DeliveryType,
    PaymentMethod PaymentMethod,
    string? CouponCode,
    string? Notes,
    DateTime? ScheduledDate) : IRequest<OrderDto>, ICommand;
