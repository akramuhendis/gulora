namespace Gulora.Contracts.Orders;

/// <summary>POST api/v1/orders/{id}/ship gövdesi (Admin / Seller).</summary>
public sealed record ShipOrderRequest(
    string CargoCompany,
    string TrackingNumber);
