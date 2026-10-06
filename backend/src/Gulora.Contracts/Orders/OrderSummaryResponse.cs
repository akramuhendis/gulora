namespace Gulora.Contracts.Orders;

/// <summary>
/// Sipariş listesi özeti — GET api/v1/orders (sayfalı).
/// Kalemler ve takip geçmişi dahil değildir.
/// </summary>
public sealed record OrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    string Status,
    string StatusLabel,
    string DeliveryType,
    string PaymentMethod,
    string PaymentStatus,
    decimal TotalAmount,
    int ItemCount,
    string? PrimaryProductName,
    string? PrimaryImageUrl,
    bool CanBeCancelled,
    bool CanBeReturned,
    DateTime CreatedAt,
    decimal? SellerAttributedTotal = null);
