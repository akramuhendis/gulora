using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfAddressRepository(GuloraDbContext db) : IAddressRepository
{
    public async Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await db.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        return list;
    }

    public Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(Address address, CancellationToken ct = default) =>
        await db.Addresses.AddAsync(address, ct).ConfigureAwait(false);

    public Task RemoveAsync(Address address, CancellationToken ct = default)
    {
        db.Addresses.Remove(address);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
