namespace Gulora.Contracts.Orders;

/// <summary>POST api/v1/orders/{id}/cancel gövdesi.</summary>
public sealed record CancelOrderRequest(
    string Reason);
