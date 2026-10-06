namespace Gulora.Contracts.Orders;

/// <summary>POST api/v1/orders gövdesi.</summary>
public sealed record CreateOrderRequest(
    Guid AddressId,
    string DeliveryType,
    string PaymentMethod,
    List<OrderItemRequest> Items,
    string? CouponCode = null,
    string? GiftCardMessage = null,
    string? GiftCardSenderName = null,
    string? GiftCardOccasion = null);

/// <summary>Sipariş kalemi satırı.</summary>
public sealed record OrderItemRequest(
    Guid ProductId,
    int Quantity);
