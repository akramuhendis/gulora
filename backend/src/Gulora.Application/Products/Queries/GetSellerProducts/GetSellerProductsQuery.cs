using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.GetSellerProducts;

/// <summary>Satıcı paneli — yalnızca oturum satıcısına ait ürünleri listeler (pasif ürünler dahil).</summary>
public sealed record GetSellerProductsQuery(
    Guid SellerUserId,
    int Page = 1,
    int PageSize = 20,
    string? Query = null,
    bool? IsActive = null,
    bool? InStock = null,
    string? SortBy = null
) : IRequest<PagedResult<ProductSummaryResponse>>;
