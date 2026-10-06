using Gulora.Domain.Common;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gulora.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository temel uygulaması.
/// Tüm özel repository'ler bu sınıftan türetilir.
/// gulora-infrastructure-contracts.mdc: IRepository&lt;T&gt; uygulaması.
/// </summary>
internal class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly GuloraDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(GuloraDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public virtual async Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(predicate, ct);

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.CountAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(predicate, ct);

    public void Add(T entity) => DbSet.Add(entity);

    public void AddRange(IEnumerable<T> entities) => DbSet.AddRange(entities);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);
}
