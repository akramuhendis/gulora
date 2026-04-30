using Gulora.Domain.Enums;

namespace Gulora.Contracts.Orders;

public record CreateOrderRequest(
    List<OrderItemRequest> Items,
    AddressRequest Address,
    DeliveryType DeliveryType,
    PaymentMethod PaymentMethod,
    string? CouponCode,
    string? Notes,
    DateTime? ScheduledDate);

public record OrderItemRequest(Guid ProductId, int Quantity, string? VariantLabel);

public record AddressRequest(
    string RecipientName, string RecipientPhone,
    string City, string District, string Line, string? PostalCode);
