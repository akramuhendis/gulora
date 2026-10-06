namespace Gulora.Contracts.Orders;

/// <summary>PATCH api/v1/orders/{id}/status gövdesi (Admin).</summary>
public sealed record UpdateOrderStatusRequest(
    string NewStatus,
    string? Note = null);
