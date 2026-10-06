using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(GuloraDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await Context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Notification>> GetByUserPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await Context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => await Context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task<int> GetTotalCountAsync(Guid userId, CancellationToken ct = default)
        => await Context.Notifications
            .CountAsync(n => n.UserId == userId, ct);
}
