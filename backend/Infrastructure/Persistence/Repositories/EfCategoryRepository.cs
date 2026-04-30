using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfCategoryRepository(GuloraDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        return list;
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug.ToLowerInvariant(), ct);
}
