using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfFavoriteRepository(GuloraDbContext db) : IFavoriteRepository
{
    public async Task<IReadOnlyList<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        return list;
    }

    public Task<Favorite?> GetAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, ct);

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default) =>
        await db.Favorites.AddAsync(favorite, ct).ConfigureAwait(false);

    public Task RemoveAsync(Favorite favorite, CancellationToken ct = default)
    {
        db.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
