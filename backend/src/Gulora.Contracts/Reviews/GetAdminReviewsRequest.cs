using Gulora.Contracts.Common;

namespace Gulora.Contracts.Reviews;

/// <summary>GET api/v1/admin/reviews sorgu parametreleri (Moderasyon kuyruğu).</summary>
public sealed record GetAdminReviewsRequest : PagedRequest
{
    /// <summary>
    /// Durum filtresi. Geçerli değerler: Pending | Approved | Rejected.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>Ürün kimliğine göre filtrele.</summary>
    public Guid? ProductId { get; init; }
}
