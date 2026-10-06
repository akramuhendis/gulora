using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class SellerCampaignPlanRepository : Repository<SellerCampaignPlan>, ISellerCampaignPlanRepository
{
    public SellerCampaignPlanRepository(GuloraDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SellerCampaignPlan>> ListBySellerAsync(Guid sellerUserId, CancellationToken ct = default)
        => await DbSet
            .Where(x => x.SellerUserId == sellerUserId)
            .OrderByDescending(x => x.StartsAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SellerCampaignPlan>> ListActiveBySellerAsync(
        Guid sellerUserId,
        DateTime nowUtc,
        CancellationToken ct = default)
        => await DbSet
            .Where(x =>
                x.SellerUserId == sellerUserId
                && x.StartsAtUtc <= nowUtc
                && x.EndsAtUtc >= nowUtc)
            .OrderBy(x => x.EndsAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<(SellerCampaignPlan Plan, string StoreSlug, string StoreName)>>
        ListActiveWithShopSlugsAsync(DateTime nowUtc, CancellationToken ct = default)
    {
        var rows = await (
            from p in DbSet
            join s in Context.Set<SellerShop>() on p.SellerUserId equals s.UserId
            where p.StartsAtUtc <= nowUtc && p.EndsAtUtc >= nowUtc
            orderby p.EndsAtUtc
            select new { Plan = p, s.StoreSlug, s.StoreName }).ToListAsync(ct);

        return rows.ConvertAll(x => (x.Plan, x.StoreSlug, x.StoreName));
    }

    public async Task<SellerCampaignPlan?> GetByIdAndSellerAsync(Guid id, Guid sellerUserId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(x => x.Id == id && x.SellerUserId == sellerUserId, ct);
}
