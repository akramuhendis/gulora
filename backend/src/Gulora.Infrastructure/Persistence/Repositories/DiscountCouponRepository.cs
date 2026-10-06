using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class DiscountCouponRepository : Repository<DiscountCoupon>, IDiscountCouponRepository
{
    public DiscountCouponRepository(GuloraDbContext context) : base(context) { }

    public override async Task<DiscountCoupon?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Context.DiscountCoupons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<DiscountCoupon?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Context.DiscountCoupons
            .IgnoreQueryFilters()   // aktif olmayan kuponlar da sorgulanabilmeli
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task<bool> HasUserUsedCouponAsync(
        Guid couponId,
        Guid userId,
        CancellationToken ct = default)
    {
        // Önce kupon kodunu al (IsActive filtresi devre dışı — pasif kupon da kontrol edilmeli)
        var couponCode = await Context.DiscountCoupons
            .IgnoreQueryFilters()
            .Where(dc => dc.Id == couponId)
            .Select(dc => dc.Code)
            .FirstOrDefaultAsync(ct);

        if (couponCode is null)
            return false;

        // Kullanıcının bu kupon koduyla geçmiş siparişi var mı?
        return await Context.Orders
            .AnyAsync(o => o.UserId == userId && o.CouponCode == couponCode, ct);
    }
}
