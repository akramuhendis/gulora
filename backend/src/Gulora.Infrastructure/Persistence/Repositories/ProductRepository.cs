using Gulora.Application.Common.Media;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(GuloraDbContext context) : base(context) { }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var s = slug.Trim();
        if (s.Length == 0)
            return null;

        var byExact = await Context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug.ToLower() == s.ToLower(), ct);

        if (byExact != null)
            return byExact;

        // URL-də tire itkisi (məs. "66gul" vs "66-gul") üçün ehtiyat uyğunluq.
        var compact = s.Replace("-", "", StringComparison.Ordinal).ToLowerInvariant();
        if (compact.Length == 0)
            return null;

        return await Context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(
                p => p.Slug.Replace("-", "").ToLower() == compact,
                ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryPagedAsync(
        Guid categoryId,
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await Context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string keyword,
        int page,
        int pageSize,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        CancellationToken ct = default)
    {
        // EF.Functions.ILike → PostgreSQL ILIKE operatörü (büyük/küçük harf duyarsız).
        // .ToLower().Contains() yerine kullanılır çünkü ILIKE pg_trgm veya B-tree
        // LOWER() fonksiyon indeksini kullanabilir; .ToLower() + .Contains() ise
        // her durumda tam tablo taraması yapar.
        var pattern = $"%{keyword}%";
        var q = Context.Products.Where(p =>
            EF.Functions.ILike(p.Name, pattern)
            || (p.Description != null && EF.Functions.ILike(p.Description, pattern)));

        if (categoryId.HasValue)
            q = q.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            q = q.Where(p => (p.DiscountedPrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            q = q.Where(p => (p.DiscountedPrice ?? p.Price) <= maxPrice.Value);

        if (inStock == true)
            q = q.Where(p => p.StockQuantity > 0);
        else if (inStock == false)
            q = q.Where(p => p.StockQuantity <= 0);

        return await q
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> SearchCountAsync(
        string keyword,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        CancellationToken ct = default)
    {
        var pattern = $"%{keyword}%";
        var q = Context.Products.Where(p =>
            EF.Functions.ILike(p.Name, pattern)
            || (p.Description != null && EF.Functions.ILike(p.Description, pattern)));

        if (categoryId.HasValue)
            q = q.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            q = q.Where(p => (p.DiscountedPrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            q = q.Where(p => (p.DiscountedPrice ?? p.Price) <= maxPrice.Value);

        if (inStock == true)
            q = q.Where(p => p.StockQuantity > 0);
        else if (inStock == false)
            q = q.Where(p => p.StockQuantity <= 0);

        return await q.CountAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllActivePagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await Context.Products
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetAllActiveCountAsync(CancellationToken ct = default)
        => await Context.Products.CountAsync(ct);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetCatalogPagedAsync(
        Guid? categoryId,
        Guid? sellerId,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        bool? inStock,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = Context.Products.AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (sellerId.HasValue)
            query = query.Where(p => p.SellerId == sellerId.Value);

        if (!string.IsNullOrWhiteSpace(brand))
        {
            var b = brand.Trim();
            query = query.Where(p => p.Brand != null && EF.Functions.ILike(p.Brand, b));
        }

        if (minPrice.HasValue)
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) <= maxPrice.Value);

        if (isFeatured == true)
            query = query.Where(p => p.IsFeatured);

        if (inStock == true)
            query = query.Where(p => p.StockQuantity > 0);
        else if (inStock == false)
            query = query.Where(p => p.StockQuantity <= 0);

        var totalCount = await query.CountAsync(ct);

        query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountedPrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.DiscountedPrice ?? p.Price),
            "rating" => query.OrderByDescending(p => p.AverageRating).ThenBy(p => p.Name),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Include(p => p.Images)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Product>> ListPromotionCandidatesAsync(
        Guid? categoryId,
        Guid? sellerId,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        bool? inStock,
        int maxCandidates,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cap = Math.Clamp(maxCandidates, 1, 5000);
        var query = Context.Products.AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (sellerId.HasValue)
            query = query.Where(p => p.SellerId == sellerId.Value);

        if (!string.IsNullOrWhiteSpace(brand))
        {
            var b = brand.Trim();
            query = query.Where(p => p.Brand != null && EF.Functions.ILike(p.Brand, b));
        }

        if (minPrice.HasValue)
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) <= maxPrice.Value);

        if (isFeatured == true)
            query = query.Where(p => p.IsFeatured);

        if (inStock == true)
            query = query.Where(p => p.StockQuantity > 0);
        else if (inStock == false)
            query = query.Where(p => p.StockQuantity <= 0);

        query = query.Where(p =>
            (p.DiscountedPrice != null && p.DiscountedPrice < p.Price)
            || (p.SellerId != null && Context.Set<SellerCampaignPlan>().Any(sc =>
                sc.SellerUserId == p.SellerId!.Value
                && sc.StartsAtUtc <= now
                && sc.EndsAtUtc >= now)));

        return await query
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .Take(cap)
            .ToListAsync(ct);
    }

    public async Task<int> CountActiveCatalogBySellerAsync(Guid sellerId, CancellationToken ct = default)
        => await Context.Products.CountAsync(p => p.SellerId == sellerId, ct);

    public async Task<bool> IsSlugTakenAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken ct = default)
        => await Context.Products
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Slug == slug
                       && (excludeProductId == null || p.Id != excludeProductId), ct);

    public async Task DecrementStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        // Atomik güncelleme — tracking yok; doğrudan SQL güncelleme
        await Context.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity),
                ct);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetBySellerPagedAsync(
        Guid sellerId,
        int page,
        int pageSize,
        string? query = null,
        bool? isActive = null,
        bool? inStock = null,
        string? sortBy = null,
        CancellationToken ct = default)
    {
        IQueryable<Product> sellerQuery = Context.Products
            .IgnoreQueryFilters()
            .Where(p => p.SellerId == sellerId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var search = $"%{query.Trim()}%";
            sellerQuery = sellerQuery.Where(p =>
                EF.Functions.ILike(p.Name, search)
                || EF.Functions.ILike(p.Slug, search)
                || (p.Sku != null && EF.Functions.ILike(p.Sku, search))
                || (p.Brand != null && EF.Functions.ILike(p.Brand, search)));
        }

        if (isActive.HasValue)
            sellerQuery = sellerQuery.Where(p => p.IsActive == isActive.Value);

        if (inStock.HasValue)
            sellerQuery = inStock.Value
                ? sellerQuery.Where(p => p.StockQuantity > 0)
                : sellerQuery.Where(p => p.StockQuantity <= 0);

        sellerQuery = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "name_asc" => sellerQuery.OrderBy(p => p.Name),
            "name_desc" => sellerQuery.OrderByDescending(p => p.Name),
            "price_asc" => sellerQuery.OrderBy(p => p.DiscountedPrice ?? p.Price),
            "price_desc" => sellerQuery.OrderByDescending(p => p.DiscountedPrice ?? p.Price),
            "stock_asc" => sellerQuery.OrderBy(p => p.StockQuantity),
            "stock_desc" => sellerQuery.OrderByDescending(p => p.StockQuantity),
            "oldest" => sellerQuery.OrderBy(p => p.CreatedAt),
            _ => sellerQuery.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await sellerQuery.CountAsync(ct);

        var items = await sellerQuery
            .Include(p => p.Images)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<int> CountBySellerAsync(Guid sellerId, CancellationToken ct = default)
        => await Context.Products
            .IgnoreQueryFilters()
            .CountAsync(p => p.SellerId == sellerId, ct);

    public async Task<(int LowStock, int OutOfStock)> CountSellerStockBucketsAsync(
        Guid sellerId,
        int lowStockThreshold,
        CancellationToken ct = default)
    {
        var low = await Context.Products
            .IgnoreQueryFilters()
            .CountAsync(
                p => p.SellerId == sellerId
                     && p.StockQuantity > 0
                     && p.StockQuantity <= lowStockThreshold,
                ct);

        var oos = await Context.Products
            .IgnoreQueryFilters()
            .CountAsync(p => p.SellerId == sellerId && p.StockQuantity <= 0, ct);

        return (low, oos);
    }

    public async Task<IReadOnlyList<(Guid Id, string Name)>> ListProductNamesForSellerAsync(
        Guid sellerId,
        IReadOnlyList<Guid> productIds,
        CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<(Guid, string)>();

        var rows = await Context.Products
            .IgnoreQueryFilters()
            .Where(p => p.SellerId == sellerId && ids.Contains(p.Id))
            .Select(p => new ValueTuple<Guid, string>(p.Id, p.Name))
            .ToListAsync(ct);

        return rows;
    }

    public async Task<IReadOnlyDictionary<Guid, string?>> GetPrimaryImageUrlsByProductIdsAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken ct = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, string?>();

        var rows = await Context.ProductImages
            .AsNoTracking()
            .Where(i => ids.Contains(i.ProductId) && i.IsActive)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new { i.ProductId, i.Url })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key,                 g => MediaUrlNormalizer.ToPublicPath(g.First().Url));
    }

    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Context.Products
            .IgnoreQueryFilters()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
