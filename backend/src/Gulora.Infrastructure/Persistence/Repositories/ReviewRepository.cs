using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(GuloraDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Review>> GetApprovedByProductPagedAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await Context.Reviews
            .Where(r => r.ProductId == productId && r.Status == Domain.Enums.ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Review>> GetByProductPagedAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await Context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetTotalCountByProductAsync(Guid productId, CancellationToken ct = default)
        => await Context.Reviews.CountAsync(r => r.ProductId == productId, ct);

    public async Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken ct = default)
        => await Context.Reviews
            .Where(r => r.Status == Domain.Enums.ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<bool> HasUserReviewedProductAsync(
        Guid userId,
        Guid productId,
        CancellationToken ct = default)
        => await Context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId, ct);

    public async Task<int> GetApprovedCountByProductAsync(
        Guid productId,
        CancellationToken ct = default)
        => await Context.Reviews
            .CountAsync(r => r.ProductId == productId
                          && r.Status == Domain.Enums.ReviewStatus.Approved, ct);

    public async Task<(double AverageRating, int ReviewCount)> GetRatingStatsAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        var stats = await Context.Reviews
            .Where(r => r.ProductId == productId && r.Status == Domain.Enums.ReviewStatus.Approved)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Average = g.Average(r => (double)r.Rating),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return stats is null
            ? (0.0, 0)
            : (Math.Round(stats.Average, 2), stats.Count);
    }
}
