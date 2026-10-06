namespace Gulora.Contracts.Orders;

/// <summary>
/// POST api/v1/orders/{id}/complete-return gövdesi (Admin / Seller).
/// Bekleyen iade talebini (<c>ReturnRequested</c>) tamamlar ve siparişi <c>Returned</c>
/// durumuna geçirir. İşlemi yapan operatör kimliği JWT'den alınır.
/// </summary>
public sealed record CompleteReturnRequest(
    string? Note = null);
