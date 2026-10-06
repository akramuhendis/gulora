using Gulora.Contracts.Common;

namespace Gulora.Contracts.Orders;

/// <summary>
/// GET api/v1/admin/orders sorgu parametreleri.
/// Admin tüm siparişleri listeler; durum, kullanıcı ve tarih aralığına göre filtreleme desteklenir.
/// </summary>
public sealed record GetAdminOrdersRequest : PagedRequest
{
    /// <summary>
    /// Sipariş durumuna göre filtrele.
    /// Geçerli değerler: Pending | Confirmed | Processing | Shipped | Delivered |
    ///                   Cancelled | ReturnRequested | Returned.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>Belirli bir kullanıcının siparişlerini filtrele.</summary>
    public Guid? UserId { get; init; }

    /// <summary>Sipariş numarasına göre ara (kısmi eşleşme).</summary>
    public string? OrderNumber { get; init; }

    /// <summary>Aralık başlangıcı — bu tarihten itibaren oluşturulan siparişler (UTC, dahil).</summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>Aralık sonu — bu tarihe kadar oluşturulan siparişler (UTC, dahil).</summary>
    public DateTime? DateTo { get; init; }
}
