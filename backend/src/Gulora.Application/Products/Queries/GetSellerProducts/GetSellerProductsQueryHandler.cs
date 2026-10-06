using Gulora.Application.SellerCampaignPlans;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Queries.GetSellerProducts;

public sealed class GetSellerProductsQueryHandler
    : IRequestHandler<GetSellerProductsQuery, PagedResult<ProductSummaryResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISellerCampaignPlanRepository _sellerCampaignPlanRepository;

    public GetSellerProductsQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISellerCampaignPlanRepository sellerCampaignPlanRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _sellerCampaignPlanRepository = sellerCampaignPlanRepository;
    }

    public async Task<PagedResult<ProductSummaryResponse>> Handle(
        GetSellerProductsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var (products, totalCount) = await _productRepository.GetBySellerPagedAsync(
            query.SellerUserId,
            page,
            pageSize,
            query.Query,
            query.IsActive,
            query.InStock,
            query.SortBy,
            ct);

        var offers = await ProductCatalogCampaignResolver.ResolveForProductsAsync(
            products,
            _sellerCampaignPlanRepository,
            ct);

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = new Dictionary<Guid, string>(categoryIds.Count);
        foreach (var catId in categoryIds)
        {
            var cat = await _categoryRepository.GetByIdAsync(catId, ct);
            if (cat is not null)
                categories[cat.Id] = cat.Name;
        }

        var items = products.Select(p => new ProductSummaryResponse(
            p.Id,
            p.Name,
            p.Slug,
            p.ShortDescription,
            p.Price,
            p.DiscountedPrice,
            p.EffectivePrice,
            p.StockQuantity,
            p.IsActive,
            p.IsAvailableForPurchase,
            p.Brand,
            p.CategoryId,
            categories.GetValueOrDefault(p.CategoryId, string.Empty),
            p.IsFeatured,
            p.AverageRating,
            p.ReviewCount,
            p.PrimaryImageUrl,
            p.CreatedAt,
            offers.GetValueOrDefault(p.Id))).ToList();

        return new PagedResult<ProductSummaryResponse>(items, totalCount, page, pageSize);
    }
}
