using Gulora.Contracts.Common;

namespace Gulora.Contracts.Orders;

/// <summary>
/// GET api/v1/seller/orders — satıcının ürünlerini içeren siparişler.
/// </summary>
public sealed record GetSellerOrdersRequest : PagedRequest
{
    /// <summary>Sipariş durumu filtresi (OrderStatus enum adı).</summary>
    public string? Status { get; init; }

    /// <summary>Sipariş numarası araması (kısmi eşleşme).</summary>
    public string? OrderNumber { get; init; }

    /// <summary>Oluşturulma tarihi başlangıcı (UTC).</summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>Oluşturulma tarihi bitişi (UTC).</summary>
    public DateTime? DateTo { get; init; }
}
