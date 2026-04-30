using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;
using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Entities;

/// <summary>
/// Müşteri siparişinin kök aggregate’i. Satır kalemleri (<see cref="OrderItem"/>)
/// ve durum geçmişi (<see cref="OrderTracking"/>) bu varlıkla birlikte tutulur.
/// Durum geçişleri yalnızca tanımlı domain metotları ile yapılır; rastgele atama yoktur.
/// </summary>
public class Order : BaseEntity
{
    private const string DefaultMoneyCurrency = "AZN";

    /// <summary>Siparişi veren kullanıcı.</summary>
    public Guid UserId { get; private set; }
    /// <summary>Anlık iş akışı durumu (beklemede, onaylandı, kargoda vb.).</summary>
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    /// <summary>Standart, aynı gün veya planlı teslimat.</summary>
    public DeliveryType DeliveryType { get; private set; }
    /// <summary>Ödeme şekli: kart, kapıda nakit, havale.</summary>
    public PaymentMethod PaymentMethod { get; private set; }
    /// <summary>Ara toplam + kargo - indirim sonrası nihai tutar.</summary>
    public decimal TotalAmount { get; private set; }
    /// <summary>Teslimat ücreti (kampanyada sıfır olabilir).</summary>
    public decimal DeliveryFee { get; private set; }
    /// <summary>Kupon veya kampanya ile uygulanan toplam indirim tutarı.</summary>
    public decimal DiscountAmount { get; private set; }
    /// <summary>Teslimat anındaki adresin kopyası; kullanıcı adresi değişse bile sipariş değişmez.</summary>
    public AddressSnapshot Address { get; private set; } = default!;
    /// <summary>Kurye veya mağaza için müşteri notu.</summary>
    public string? Notes { get; private set; }
    /// <summary>Uygulanan kupon kodu (varsa).</summary>
    public string? CouponCode { get; private set; }
    /// <summary>Planlı teslimat için seçilen tarih (isteğe bağlı).</summary>
    public DateTime? ScheduledDate { get; private set; }

    private readonly List<OrderItem> _items = [];
    /// <summary>Sipariş satırları salt okunur görünüm.</summary>
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderTracking> _tracking = [];
    /// <summary>Durum değişiklikleri zaman çizelgesi.</summary>
    public IReadOnlyCollection<OrderTracking> Tracking => _tracking.AsReadOnly();

    private Order() { }

    /// <summary>
    /// Yeni sipariş oluşturur. Satırlar <see cref="OrderLine"/> olarak gelir;
    /// her biri bu siparişin <see cref="BaseEntity.Id"/> değeri ile <see cref="OrderItem"/> olarak eklenir
    /// (dışarıdan yanlış OrderId ile satır gelmesi engellenir).
    /// </summary>
    /// <param name="userId">Sipariş sahibi.</param>
    /// <param name="lines">En az bir satır içermelidir.</param>
    /// <param name="address">Teslimat adresi anlık görüntüsü.</param>
    /// <param name="deliveryType">Teslimat modu (standart, aynı gün, planlı).</param>
    /// <param name="paymentMethod">Ödeme kanalı.</param>
    /// <param name="deliveryFee">Kargo ücreti; negatif olamaz.</param>
    /// <param name="discountAmount">Kupon indirimi; negatif olamaz, ara toplam + kargodan büyük olamaz.</param>
    /// <param name="couponCode">Uygulanan kupon kodu; yoksa null.</param>
    /// <param name="notes">Müşteri notu; yoksa null.</param>
    /// <param name="scheduledDate">Planlı teslimat tarihi; yoksa null.</param>
    public static Order Create(
        Guid userId,
        IReadOnlyList<OrderLine> lines,
        AddressSnapshot address,
        DeliveryType deliveryType,
        PaymentMethod paymentMethod,
        decimal deliveryFee,
        decimal discountAmount = 0,
        string? couponCode = null,
        string? notes = null,
        DateTime? scheduledDate = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryFee);
        ArgumentOutOfRangeException.ThrowIfNegative(discountAmount);
        address.Validate();
        if (deliveryType == DeliveryType.Scheduled && !scheduledDate.HasValue)
            throw new DomainException("Planlı çatdırılma üçün tarix seçilməlidir.");
        if (lines.Count == 0)
            throw new DomainException("Sifariş ən azı bir məhsul ehtiva etməlidir.");

        foreach (var line in lines)
            line.EnsureValid();

        Money subtotalMoney = Money.Zero;
        foreach (var line in lines)
        {
            var lineMoney = new Money(checked(line.UnitPrice * line.Quantity), DefaultMoneyCurrency);
            subtotalMoney = subtotalMoney.Add(lineMoney);
        }

        if (discountAmount > subtotalMoney.Amount + deliveryFee)
            throw new DomainException("Endirim məbləği sifariş məbləğindən böyük ola bilməz.");

        var afterDelivery = subtotalMoney.Add(new Money(deliveryFee, DefaultMoneyCurrency));
        var afterDiscount = afterDelivery.Subtract(new Money(discountAmount, DefaultMoneyCurrency));
        var total = afterDiscount.Amount;

        var order = new Order
        {
            UserId = userId,
            Address = address,
            DeliveryType = deliveryType,
            PaymentMethod = paymentMethod,
            DeliveryFee = deliveryFee,
            DiscountAmount = discountAmount,
            TotalAmount = total,
            CouponCode = couponCode,
            Notes = notes,
            ScheduledDate = scheduledDate
        };

        // BaseEntity.Id oluşturulduğu için satırlar doğru OrderId ile bağlanır
        foreach (var line in lines)
        {
            order._items.Add(OrderItem.Create(
                order.Id,
                line.ProductId,
                line.ProductName,
                line.ProductImageUrl,
                line.Quantity,
                line.UnitPrice,
                line.VariantLabel,
                line.Addons));
        }

        order._tracking.Add(OrderTracking.Create(order.Id, OrderStatus.Pending, "Sifariş qəbul edildi."));
        order.RegisterDomainEvent(new OrderCreatedDomainEvent(order.Id));

        return order;
    }

    /// <summary>Sipariş tutarları uygulama genelinde AZN kabul edilir; <see cref="Money"/> ile okunur.</summary>
    public Money TotalAsMoney() => new(TotalAmount, DefaultMoneyCurrency);

    /// <summary>Kargo ücreti (AZN).</summary>
    public Money DeliveryFeeAsMoney() => new(DeliveryFee, DefaultMoneyCurrency);

    /// <summary>İndirim tutarı (AZN).</summary>
    public Money DiscountAmountAsMoney() => new(DiscountAmount, DefaultMoneyCurrency);

    /// <summary>Ödeme onayı sonrası beklemeden onaylandı durumuna geçer.</summary>
    public void Confirm()
    {
        EnsureStatus(OrderStatus.Pending);
        TransitionTo(OrderStatus.Confirmed, "Sifariş təsdiqləndi.");
    }

    /// <summary>Depo / mağaza siparişi hazırlamaya başlar.</summary>
    public void StartPreparing()
    {
        EnsureStatus(OrderStatus.Confirmed);
        TransitionTo(OrderStatus.Preparing, "Sifariş hazırlanır.");
    }

    /// <summary>Kargoya veya kuryeye teslim edildi olarak işaretlenir.</summary>
    public void Ship()
    {
        EnsureStatus(OrderStatus.Preparing);
        TransitionTo(OrderStatus.Shipped, "Sifariş kuryer tərəfindən götürüldü.");
    }

    /// <summary>Müşteriye teslim tamamlandı.</summary>
    public void Deliver()
    {
        EnsureStatus(OrderStatus.Shipped);
        TransitionTo(OrderStatus.Delivered, "Sifariş çatdırıldı.");
    }

    /// <summary>
    /// Kargoya çıkmadan önceki aşamalarda iptal edilebilir.
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Sifariş artıq ləğv edilib.");
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new DomainException("Göndərilmiş və ya çatdırılmış sifariş ləğv edilə bilməz.");
        if (Status is OrderStatus.ReturnRequested or OrderStatus.Returned)
            throw new DomainException("İadə prosesində olan sifariş ləğv edilə bilməz.");
        var previous = Status;
        Status = OrderStatus.Cancelled;
        _tracking.Add(OrderTracking.Create(Id, Status, "Sifariş ləğv edildi."));
        RegisterDomainEvent(new OrderStatusChangedDomainEvent(Id, previous, Status));
        SetUpdated();
    }

    /// <summary>Teslim edildikten sonra iade talebi açar.</summary>
    public void RequestReturn(string reason)
    {
        EnsureStatus(OrderStatus.Delivered);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        TransitionTo(OrderStatus.ReturnRequested, $"İadə tələb edildi: {reason.Trim()}");
    }

    /// <summary>İade onaylandıktan sonra siparişi tamamlanmış iade durumuna alır.</summary>
    public void CompleteReturn()
    {
        EnsureStatus(OrderStatus.ReturnRequested);
        TransitionTo(OrderStatus.Returned, "İadə tamamlandı.");
    }

    private void TransitionTo(OrderStatus nextStatus, string trackingMessage)
    {
        var previous = Status;
        Status = nextStatus;
        _tracking.Add(OrderTracking.Create(Id, Status, trackingMessage));
        RegisterDomainEvent(new OrderStatusChangedDomainEvent(Id, previous, Status));
        SetUpdated();
    }

    /// <summary>Beklenen durumda değilse anlamlı domain hatası fırlatır.</summary>
    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new DomainException($"Bu əməliyyat üçün sifariş '{expected}' statusunda olmalıdır.");
    }
}
