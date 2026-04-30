namespace Gulora.Contracts.Orders;

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? VariantLabel);
