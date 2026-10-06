using Gulora.Contracts.Common;

namespace Gulora.Contracts.Reviews;

/// <summary>GET api/v1/products/{productId}/reviews sorgu parametreleri.</summary>
public sealed record GetProductReviewsRequest : PagedRequest
{
    /// <summary>Puan filtresi (1–5). Boşsa tüm puanlar.</summary>
    public int? Rating { get; init; }
}
