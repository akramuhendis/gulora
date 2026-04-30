using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfOrderRepository(GuloraDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Orders
            .Include(o => o.Items)
            .Include(o => o.Tracking)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Tracking)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        return list;
    }

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await db.Orders.AddAsync(order, ct).ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
