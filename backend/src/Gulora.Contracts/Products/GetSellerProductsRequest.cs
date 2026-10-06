using Gulora.Contracts.Common;

namespace Gulora.Contracts.Products;

/// <summary>GET api/v1/seller/products — oturum satıcısına ait ürün envanteri.</summary>
public sealed record GetSellerProductsRequest : PagedRequest
{
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public bool? InStock { get; init; }
    public string? SortBy { get; init; }
}
