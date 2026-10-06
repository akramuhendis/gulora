using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProducts;

/// <summary>Katalog ürünlerini filtre, sıralama ve sayfalama ile getirir.</summary>
public sealed record GetProductsQuery(
    Guid? CategoryId = null,
    Guid? SellerId = null,
    string? Brand = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsFeatured = null,
    bool? InStock = null,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 20,
    bool? OnPromotion = null
) : IRequest<PagedResult<ProductSummaryResponse>>;
