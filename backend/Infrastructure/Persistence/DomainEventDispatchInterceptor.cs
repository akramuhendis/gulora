using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Common;
using Gulora.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Gulora.Infrastructure.Persistence;

/// <summary>
/// <see cref="GuloraDbContext"/> saxlaması bitdikdən sonra bütün izlənən varlıqların
/// <see cref="BaseEntity.PopDomainEvents"/> nəticəsini <see cref="IDomainEventDispatcher"/> ilə yönləndirir.
/// </summary>
public sealed class DomainEventDispatchInterceptor(IDomainEventDispatcher dispatcher) : ISaveChangesInterceptor
{
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) => result;

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(result);

    public int SavedChanges(SaveChangesCompletedEventData eventData, int result) =>
        SavedChangesAsync(eventData, result, CancellationToken.None).AsTask().GetAwaiter().GetResult();

    public async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not GuloraDbContext db)
            return result;

        var batch = new List<IDomainEvent>();
        foreach (var entry in db.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Detached)
                continue;
            foreach (var e in entry.Entity.PopDomainEvents())
                batch.Add(e);
        }

        if (batch.Count > 0)
            await dispatcher.DispatchAsync(batch, cancellationToken).ConfigureAwait(false);

        return result;
    }
}
