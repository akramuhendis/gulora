using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfProductRepository(GuloraDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug.ToLowerInvariant(), ct);

    public async Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(
        string? categorySlug,
        decimal? minPrice,
        decimal? maxPrice,
        string? tag,
        string? badge,
        bool? isSameDay,
        bool? isFreeDelivery,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var q = db.Products.AsQueryable();

        q = q.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            var cat = await db.Categories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Slug == categorySlug.ToLowerInvariant(), ct)
                .ConfigureAwait(false);
            if (cat is null)
                return (Array.Empty<Product>(), 0);
            q = q.Where(p => p.CategoryId == cat.Id);
        }

        if (minPrice is decimal min)
            q = q.Where(p => p.Price >= min);
        if (maxPrice is decimal max)
            q = q.Where(p => p.Price <= max);
        if (!string.IsNullOrWhiteSpace(badge))
            q = q.Where(p => p.Badge == badge);
        if (isSameDay is true)
            q = q.Where(p => p.IsSameDay);
        if (isSameDay is false)
            q = q.Where(p => !p.IsSameDay);
        if (isFreeDelivery is true)
            q = q.Where(p => p.IsFreeDelivery);
        if (isFreeDelivery is false)
            q = q.Where(p => !p.IsFreeDelivery);
        if (!string.IsNullOrWhiteSpace(tag))
            q = q.Where(p => p.Tags.Contains(tag));

        var total = await q.CountAsync(ct).ConfigureAwait(false);

        var ordered = (sortBy?.ToLowerInvariant()) switch
        {
            "price" => q.OrderBy(p => p.Price),
            "price_desc" => q.OrderByDescending(p => p.Price),
            "name" => q.OrderBy(p => p.Name),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, total);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        var q = query.Trim();
        if (q.Length == 0)
            return Array.Empty<Product>();

        limit = Math.Clamp(limit, 1, 50);
        return await db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Name.Contains(q))
            .OrderBy(p => p.Name)
            .Take(limit)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return Array.Empty<Product>();

        return await db.Products
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
