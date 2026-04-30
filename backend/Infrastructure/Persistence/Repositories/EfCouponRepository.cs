using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfCouponRepository(GuloraDbContext db) : ICouponRepository
{
    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return db.Coupons.FirstOrDefaultAsync(c => c.Code == normalized, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
