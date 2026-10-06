namespace Gulora.Contracts.Orders;

/// <summary>Sipariş kalemi yanıtı.</summary>
public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductSku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    string? ProductImageUrl = null);
