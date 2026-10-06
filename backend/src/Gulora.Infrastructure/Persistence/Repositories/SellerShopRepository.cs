using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class SellerShopRepository : Repository<SellerShop>, ISellerShopRepository
{
    public SellerShopRepository(GuloraDbContext context) : base(context) { }

    public async Task<SellerShop?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<SellerShop?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var s = slug.Trim().ToLowerInvariant();
        if (s.Length == 0)
            return null;

        return await DbSet.FirstOrDefaultAsync(x => x.StoreSlug == s, ct);
    }

    public async Task<bool> IsSlugTakenByOtherSellerAsync(
        string slug,
        Guid? excludeUserId = null,
        CancellationToken ct = default)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return await DbSet.AnyAsync(
            x => x.StoreSlug == normalized
                 && (excludeUserId == null || x.UserId != excludeUserId.Value),
            ct);
    }
}
