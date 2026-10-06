using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.SearchProducts;

/// <summary>Ürün adı və ya təsvirində açar söz axtarışı; isteğe bağlı süzgəclər.</summary>
public sealed record SearchProductsQuery(
    string Keyword,
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStock = null
) : IRequest<PagedResult<ProductSummaryResponse>>;
