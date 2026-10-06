using Gulora.Application.SellerCampaignPlans;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductSummaryResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISellerCampaignPlanRepository _sellerCampaignPlanRepository;

    public SearchProductsQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISellerCampaignPlanRepository sellerCampaignPlanRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _sellerCampaignPlanRepository = sellerCampaignPlanRepository;
    }

    public async Task<PagedResult<ProductSummaryResponse>> Handle(
        SearchProductsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var products = await _productRepository.SearchAsync(
            query.Keyword,
            page,
            pageSize,
            query.CategoryId,
            query.MinPrice,
            query.MaxPrice,
            query.InStock,
            ct);
        var totalCount = await _productRepository.SearchCountAsync(
            query.Keyword,
            query.CategoryId,
            query.MinPrice,
            query.MaxPrice,
            query.InStock,
            ct);

        var offers = await ProductCatalogCampaignResolver.ResolveForProductsAsync(
            products,
            _sellerCampaignPlanRepository,
            ct);

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = new Dictionary<Guid, string>();
        foreach (var catId in categoryIds)
        {
            var cat = await _categoryRepository.GetByIdAsync(catId, ct);
            if (cat is not null) categories[cat.Id] = cat.Name;
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
