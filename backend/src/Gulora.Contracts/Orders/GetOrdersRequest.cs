using Gulora.Contracts.Common;

namespace Gulora.Contracts.Orders;

/// <summary>GET api/v1/orders sorgu parametreleri (kullanıcı sipariş listesi).</summary>
public sealed record GetOrdersRequest : PagedRequest
{
    /// <summary>
    /// Sipariş durumuna göre filtrele.
    /// Geçerli değerler: Pending | Confirmed | Processing | Shipped | Delivered |
    ///                   Cancelled | ReturnRequested | Returned.
    /// </summary>
    public string? Status { get; init; }
}
