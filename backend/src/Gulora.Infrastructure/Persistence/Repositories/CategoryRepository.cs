using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(GuloraDbContext context) : base(context) { }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await Context.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
        => await Context.Categories
            .Where(c => c.ParentId == null)
            .Include(c => c.Children)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(
        Guid parentId,
        CancellationToken ct = default)
        => await Context.Categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

    public async Task<int> GetProductCountAsync(Guid categoryId, CancellationToken ct = default)
        => await Context.Products
            .CountAsync(p => p.CategoryId == categoryId, ct);
}
