using Gulora.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gulora.Infrastructure.Persistence;

public sealed class EfUnitOfWork(GuloraDbContext db) : IUnitOfWork
{
    public async Task<T> RunAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var result = await operation().ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task RunAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await RunAsync(async () =>
        {
            await operation().ConfigureAwait(false);
            return 0;
        }, cancellationToken).ConfigureAwait(false);
    }
}
