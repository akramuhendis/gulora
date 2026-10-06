using Gulora.Application.Common.Media;
using Gulora.Application.SellerCampaignPlans;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, PagedResult<ProductSummaryResponse>>
{
    private const int PromotionCandidateCap = 3500;

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISellerCampaignPlanRepository _sellerCampaignPlanRepository;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISellerCampaignPlanRepository sellerCampaignPlanRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _sellerCampaignPlanRepository = sellerCampaignPlanRepository;
    }

    public async Task<PagedResult<ProductSummaryResponse>> Handle(
        GetProductsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var sortKey = string.IsNullOrWhiteSpace(query.SortBy)
            ? null
            : query.SortBy.Trim().ToLowerInvariant();

        if (query.OnPromotion == true)
            return await HandleOnPromotionAsync(query, sortKey, page, pageSize, ct);

        var (products, totalCount) = await _productRepository.GetCatalogPagedAsync(
            query.CategoryId,
            query.SellerId,
            query.Brand,
            query.MinPrice,
            query.MaxPrice,
            query.IsFeatured,
            query.InStock,
            sortKey,
            page,
            pageSize,
            ct);

        var offers = await ProductCatalogCampaignResolver.ResolveForProductsAsync(
            products,
            _sellerCampaignPlanRepository,
            ct);

        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await LoadCategoryNamesAsync(categoryIds, ct);

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
            MediaUrlNormalizer.ToPublicPath(p.PrimaryImageUrl),
            p.CreatedAt,
            offers.GetValueOrDefault(p.Id))).ToList();

        return new PagedResult<ProductSummaryResponse>(items, totalCount, page, pageSize);
    }

    private async Task<PagedResult<ProductSummaryResponse>> HandleOnPromotionAsync(
        GetProductsQuery query,
        string? sortKey,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var candidates = await _productRepository.ListPromotionCandidatesAsync(
            query.CategoryId,
            query.SellerId,
            query.Brand,
            query.MinPrice,
            query.MaxPrice,
            query.IsFeatured,
            query.InStock,
            PromotionCandidateCap,
            ct);

        if (candidates.Count == 0)
            return new PagedResult<ProductSummaryResponse>([], 0, page, pageSize);

        var offers = await ProductCatalogCampaignResolver.ResolveForProductsAsync(
            candidates,
            _sellerCampaignPlanRepository,
            ct);

        var categoryIds = candidates.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await LoadCategoryNamesAsync(categoryIds, ct);

        var filtered = candidates
            .Select(p => new ProductSummaryResponse(
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
                MediaUrlNormalizer.ToPublicPath(p.PrimaryImageUrl),
                p.CreatedAt,
                offers.GetValueOrDefault(p.Id)))
            .Where(IsOnPromotion)
            .ToList();

        var sorted = SortPromotionItems(filtered, sortKey).ToList();
        var totalCount = sorted.Count;
        var pageItems = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<ProductSummaryResponse>(pageItems, totalCount, page, pageSize);
    }

    private async Task<Dictionary<Guid, string>> LoadCategoryNamesAsync(
        IReadOnlyList<Guid> categoryIds,
        CancellationToken ct)
    {
        var categories = new Dictionary<Guid, string>(categoryIds.Count);
        foreach (var catId in categoryIds)
        {
            var cat = await _categoryRepository.GetByIdAsync(catId, ct);
            if (cat is not null) categories[cat.Id] = cat.Name;
        }

        return categories;
    }

    private static bool IsOnPromotion(ProductSummaryResponse x)
        => x.SellerCampaign is not null
           || (x.DiscountedPrice is not null && x.DiscountedPrice.Value < x.Price);

    private static IEnumerable<ProductSummaryResponse> SortPromotionItems(
        IReadOnlyList<ProductSummaryResponse> items,
        string? sortKey)
    {
        return sortKey switch
        {
            "price_asc" => items.OrderBy(x => x.EffectivePrice).ThenBy(x => x.Name),
            "price_desc" => items.OrderByDescending(x => x.EffectivePrice).ThenBy(x => x.Name),
            "rating" => items.OrderByDescending(x => x.AverageRating).ThenBy(x => x.Name),
            "name_asc" => items.OrderBy(x => x.Name),
            "name_desc" => items.OrderByDescending(x => x.Name),
            "newest" => items.OrderByDescending(x => x.CreatedAt),
            _ => items.OrderByDescending(x => x.CreatedAt)
        };
    }
}
