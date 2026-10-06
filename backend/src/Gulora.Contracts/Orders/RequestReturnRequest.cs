namespace Gulora.Contracts.Orders;

/// <summary>POST api/v1/orders/{id}/return gövdesi.</summary>
public sealed record RequestReturnRequest(
    string Reason);
