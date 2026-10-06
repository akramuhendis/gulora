namespace Gulora.Contracts.Orders;

/// <summary>
/// Sipariş yanıtı — GET api/v1/orders/{id} ve liste sonuçlarında kullanılır.
/// Domain entity doğrudan dışa aktarılmaz; bu record kablo sözleşmesidir.
/// </summary>
public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    string Status,
    string StatusLabel,
    string DeliveryType,
    string PaymentMethod,
    string PaymentStatus,
    decimal SubTotal,
    decimal DeliveryFee,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? CouponCode,
    string? TrackingNumber,
    string? CargoCompany,
    string? CancellationReason,
    DateTime? DeliveredAt,
    bool CanBeCancelled,
    bool CanBeReturned,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    AddressSnapshotResponse Address,
    IReadOnlyList<OrderItemResponse> Items,
    IReadOnlyList<OrderTrackingResponse> Trackings,
    string? GiftCardMessage,
    string? GiftCardSenderName,
    string? GiftCardOccasion);
