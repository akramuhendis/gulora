using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// <see cref="Category"/> ağaç/liste erişim sözleşmesi. Menü kategorileri ve katalog filtreleme
/// (slug) için salt okunur sorgulama odaklıdır; yazma yönetim paneli başka uçlarda olabilir.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>Tüm kategoriler; sıra ve ağaç yapısı sorgu tarafında/EF konfigurasyonundadır.</summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Yönetim veya iç referanslar için tekil kategori (isteğe bağlı uç noktalar).</summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>URL/filtre için SEO slug ile kategori; ürün listesinde dış anahtar olarak.</summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
