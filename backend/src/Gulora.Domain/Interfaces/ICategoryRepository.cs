using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Category hiyerarşi sorgulama operasyonları.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>Slug ile kategori getirir.</summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Kök kategorileri (parent olmayan) getirir.</summary>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);

    /// <summary>Belirtilen kategorinin alt kategorilerini getirir.</summary>
    Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>Kategorinin tüm ürün sayısını döner (aktif ürünler).</summary>
    Task<int> GetProductCountAsync(Guid categoryId, CancellationToken ct = default);
}
