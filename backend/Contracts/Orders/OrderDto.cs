using Gulora.Domain.Enums;

namespace Gulora.Contracts.Orders;

public record OrderDto(
    Guid Id,
    OrderStatus Status,
    string StatusLabel,
    DeliveryType DeliveryType,
    PaymentMethod PaymentMethod,
    decimal TotalAmount,
    decimal DeliveryFee,
    decimal DiscountAmount,
    AddressSnapshotDto Address,
    string? Notes,
    string? CouponCode,
    DateTime? ScheduledDate,
    List<OrderItemDto> Items,
    List<OrderTrackingDto> Tracking,
    DateTime CreatedAt);

public record AddressSnapshotDto(
    string RecipientName, string RecipientPhone,
    string City, string District, string Line, string? PostalCode);

public record OrderTrackingDto(
    OrderStatus Status, string StatusLabel,
    string Message, DateTime Timestamp);
