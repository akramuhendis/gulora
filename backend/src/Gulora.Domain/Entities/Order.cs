using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;
using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Entities;

/// <summary>
/// Sipariş aggregate root'u — Gülora'nın merkezi iş nesnesi.
/// Durum makinesi bu sınıfta korunur; geçersiz geçişler exception fırlatır.
/// Alt entity'ler (OrderItem, OrderTracking) yalnızca bu kök aracılığıyla değiştirilir.
/// AddressSnapshot değişmez — sipariş oluşturulurken adres JSON olarak kopyalanır.
/// </summary>
public sealed class Order : BaseAggregateRoot
{
    public Guid UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public DeliveryType DeliveryType { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>Ürün kalemleri toplamı (indirim ve kargo öncesi).</summary>
    public decimal SubTotal { get; private set; }

    /// <summary>Kargo ücreti.</summary>
    public decimal DeliveryFee { get; private set; }

    /// <summary>Kupon/indirim tutarı.</summary>
    public decimal DiscountAmount { get; private set; }

    /// <summary>Son ödeme tutarı = SubTotal + DeliveryFee − DiscountAmount.</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Kullanılan kupon kodu (varsa).</summary>
    public string? CouponCode { get; private set; }

    /// <summary>Çiçək kartı / hədiyyə kartı mətni (müştəri mesajı).</summary>
    public string? GiftCardMessage { get; private set; }

    /// <summary>Kartda çap olunacaq göndərən adı (boş olsa anonim).</summary>
    public string? GiftCardSenderName { get; private set; }

    /// <summary>Göndərmə məqsədi (məs. doğum günü).</summary>
    public string? GiftCardOccasion { get; private set; }

    /// <summary>Sipariş anındaki adres anlık görüntüsü (JSON ile saklanır).</summary>
    public AddressSnapshot AddressSnapshot { get; private set; } = default!;

    /// <summary>Kargo takip numarası.</summary>
    public string? TrackingNumber { get; private set; }

    /// <summary>Kargo firması.</summary>
    public string? CargoCompany { get; private set; }

    /// <summary>İptal gerekçesi.</summary>
    public string? CancellationReason { get; private set; }

    /// <summary>Teslim tarihi (tahmini veya gerçek).</summary>
    public DateTime? DeliveredAt { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderTracking> _trackings = [];
    public IReadOnlyList<OrderTracking> Trackings => _trackings.AsReadOnly();

    private Order() { }

    /// <summary>
    /// Yeni sipariş oluşturur, tutarları hesaplar ve <see cref="OrderCreatedEvent"/> yayınlar.
    /// </summary>
    public static Order Create(
        Guid userId,
        string orderNumber,
        AddressSnapshot address,
        DeliveryType deliveryType,
        PaymentMethod paymentMethod,
        IEnumerable<(Guid ProductId, string ProductName, string? Sku, decimal UnitPrice, int Quantity)> items,
        decimal deliveryFee = 0m,
        decimal discountAmount = 0m,
        string? couponCode = null,
        string? giftCardMessage = null,
        string? giftCardSenderName = null,
        string? giftCardOccasion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new BusinessRuleException("Sipariş en az bir kalem içermelidir.");

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            AddressSnapshot = address,
            DeliveryType = deliveryType,
            PaymentMethod = paymentMethod,
            PaymentStatus = paymentMethod == PaymentMethod.CashOnDelivery
                ? PaymentStatus.Pending
                : PaymentStatus.Pending,
            Status = OrderStatus.Pending,
            DeliveryFee = deliveryFee,
            DiscountAmount = discountAmount,
            CouponCode = couponCode?.Trim().ToUpperInvariant(),
            GiftCardMessage = NormalizeGiftField(giftCardMessage, 2000),
            GiftCardSenderName = NormalizeGiftField(giftCardSenderName, 120),
            GiftCardOccasion = NormalizeGiftField(giftCardOccasion, 80)
        };

        foreach (var item in itemList)
        {
            var orderItem = OrderItem.Create(
                order.Id,
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity,
                item.Sku);

            order._items.Add(orderItem);
        }

        order.SubTotal = order._items.Sum(i => i.LineTotal);
        order.TotalAmount = order.SubTotal + order.DeliveryFee - order.DiscountAmount;

        if (order.TotalAmount < 0)
            throw new BusinessRuleException("Sipariş toplam tutarı negatif olamaz.");

        order._trackings.Add(OrderTracking.Create(order.Id, OrderStatus.Pending, "Sipariş alındı."));

        order.AddDomainEvent(new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.OrderNumber,
            order.TotalAmount,
            order.PaymentMethod,
            order.DeliveryType));

        return order;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Durum geçiş metodları — izin verilmeyen geçişler exception fırlatır.
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Siparişi onaylar (Pending → Confirmed).</summary>
    public void Confirm(Guid? operatorId = null)
    {
        EnsureStatus(OrderStatus.Pending, "onaylamak");
        ChangeStatus(OrderStatus.Confirmed, "Sipariş onaylandı.", operatorId);
    }

    /// <summary>Siparişi işleme alır (Confirmed → Processing).</summary>
    public void StartProcessing(Guid? operatorId = null)
    {
        EnsureStatus(OrderStatus.Confirmed, "işleme almak");
        ChangeStatus(OrderStatus.Processing, "Sipariş hazırlanıyor.", operatorId);
    }

    /// <summary>Kargoya verilir (Processing → Shipped).</summary>
    public void Ship(string cargoCompany, string trackingNumber, Guid? operatorId = null)
    {
        EnsureStatus(OrderStatus.Processing, "kargoya vermek");
        ArgumentException.ThrowIfNullOrWhiteSpace(cargoCompany);
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);

        CargoCompany = cargoCompany.Trim();
        TrackingNumber = trackingNumber.Trim();

        ChangeStatus(
            OrderStatus.Shipped,
            $"Kargo firması: {cargoCompany}, Takip: {trackingNumber}",
            operatorId);
    }

    /// <summary>Teslim edildi (Shipped → Delivered).</summary>
    public void MarkDelivered(Guid? operatorId = null)
    {
        EnsureStatus(OrderStatus.Shipped, "teslim edildi olarak işaretlemek");
        DeliveredAt = DateTime.UtcNow;
        ChangeStatus(OrderStatus.Delivered, "Sipariş teslim edildi.", operatorId);
    }

    /// <summary>
    /// İptal edilir (→ Cancelled).
    /// Müştəri: yalnız <see cref="OrderStatus.Pending"/> (satıcı təsdiqi gözləyərkən).
    /// Admin: Pending və ya Confirmed.
    /// </summary>
    public void Cancel(string reason, Guid? operatorId = null, bool allowAfterSellerConfirm = false)
    {
        if (allowAfterSellerConfirm)
        {
            if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                throw new BusinessRuleException(
                    $"'{Status}' durumundaki sipariş iptal edilemez. Yalnızca Pending veya Confirmed siparişler iptal edilebilir.");
        }
        else if (Status != OrderStatus.Pending)
        {
            throw new BusinessRuleException(
                Status == OrderStatus.Confirmed
                    ? "Sipariş satıcı tarafından onaylandıktan sonra müşteri tarafından iptal edilemez."
                    : $"'{Status}' durumundaki sipariş müşteri tarafından iptal edilemez. Yalnızca onay bekleyen (Pending) siparişler iptal edilebilir.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        CancellationReason = reason.Trim();

        ChangeStatus(OrderStatus.Cancelled, $"İptal gerekçesi: {reason}", operatorId);

        AddDomainEvent(new OrderCancelledEvent(Id, UserId, OrderNumber, CancellationReason));
    }

    /// <summary>İade talebi açılır (Delivered → ReturnRequested).</summary>
    public void RequestReturn(string reason, Guid? operatorId = null)
    {
        EnsureStatus(OrderStatus.Delivered, "iade talebi açmak");
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ChangeStatus(OrderStatus.ReturnRequested, $"İade talebi: {reason}", operatorId);
    }

    /// <summary>İade tamamlanır (ReturnRequested → Returned).</summary>
    public void CompleteReturn(Guid? operatorId = null, string? completionNote = null)
    {
        EnsureStatus(OrderStatus.ReturnRequested, "iade tamamlamak");
        var note = string.IsNullOrWhiteSpace(completionNote)
            ? "İade tamamlandı."
            : $"İade tamamlandı. Not: {completionNote.Trim()}";
        ChangeStatus(OrderStatus.Returned, note, operatorId);
    }

    /// <summary>Ödeme durumunu günceller (ödeme sağlayıcısı webhook'u sonrası).</summary>
    public void UpdatePaymentStatus(PaymentStatus paymentStatus)
    {
        PaymentStatus = paymentStatus;
        MarkUpdated();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Yardımcı metodlar
    // ──────────────────────────────────────────────────────────────────────────

    private void EnsureStatus(OrderStatus required, string action)
    {
        if (Status != required)
            throw new BusinessRuleException(
                $"Siparişi '{action}' için durum '{required}' olmalıdır. Mevcut durum: '{Status}'.");
    }

    private void ChangeStatus(OrderStatus newStatus, string? note, Guid? changedByUserId)
    {
        var oldStatus = Status;
        Status = newStatus;
        MarkUpdated();

        _trackings.Add(OrderTracking.Create(
            Id,
            newStatus,
            note,
            CargoCompany,
            TrackingNumber,
            changedByUserId));

        AddDomainEvent(new OrderStatusChangedEvent(Id, UserId, OrderNumber, oldStatus, newStatus));
    }

    /// <summary>Müştərinin iptal edə biləcəyi hal (satıcı təsdiqi gözləyərkən).</summary>
    public bool CanBeCancelled => Status == OrderStatus.Pending;

    /// <summary>Siparişin iade talebine uygun olup olmadığını kontrol eder.</summary>
    public bool CanBeReturned =>
        Status == OrderStatus.Delivered;

    private static string? NormalizeGiftField(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var t = value.Trim();
        return t.Length <= maxLen ? t : t[..maxLen];
    }
}
