using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfReviewRepository(GuloraDbContext db) : IReviewRepository
{
    public async Task<(IReadOnlyList<Review> Items, int Total)> GetByProductIdAsync(
        Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId && !r.IsDeleted && !r.IsHidden);

        var total = await baseQuery.CountAsync(ct).ConfigureAwait(false);

        var items = await baseQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, total);
    }

    public Task<bool> HasReviewedAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        db.Reviews.AnyAsync(
            r => r.UserId == userId && r.ProductId == productId && !r.IsDeleted && !r.IsHidden,
            ct);

    public async Task AddAsync(Review review, CancellationToken ct = default) =>
        await db.Reviews.AddAsync(review, ct).ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
