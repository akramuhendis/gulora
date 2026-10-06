namespace Gulora.Contracts.Orders;

/// <summary>
/// Sipariş durum geçmişi satırı.
/// ChangedByUserId: işlemi gerçekleştiren admin/satıcı kimliği; sistem otomasyonlarında null olabilir.
/// </summary>
public sealed record OrderTrackingResponse(
    Guid Id,
    string Status,
    string StatusLabel,
    string? Note,
    string? CargoCompany,
    string? TrackingNumber,
    Guid? ChangedByUserId,
    DateTime CreatedAt);
