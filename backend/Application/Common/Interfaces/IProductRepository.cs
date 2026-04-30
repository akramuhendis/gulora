using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// <see cref="Product"/> (ürün) kataloğuna ait sorgu ve stok/durum güncellemeleri. Liste, sayfalama,
/// slug ile bulma, ID listesi ile toplu yükleme (siparişte stok azaltma) senaryolarını kapsar.
/// </summary>
public interface IProductRepository
{
    /// <summary>Tekil ürün: detay, sepet, favori ekleme doğrulaması.</summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>SEO dostu <paramref name="slug"/> ile katalog/ürün sayfası açılırken kullanılır.</summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Çok alanlı filtre + sıralama + <paramref name="page"/>, <paramref name="pageSize"/> ile paged katalog sorgusu.</summary>
    Task<(IReadOnlyList<Product> Items, int Total)> GetPagedAsync(
        string? categorySlug, decimal? minPrice, decimal? maxPrice,
        string? tag, string? badge, bool? isSameDay, bool? isFreeDelivery,
        string? sortBy, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Üst arama (autocomplete/limit'li) için metin sorgusuna dayalı hızlı ürün listesi.</summary>
    Task<IReadOnlyList<Product>> SearchAsync(string query, int limit, CancellationToken ct = default);

    /// <summary>Çok satırlı bir siparişte geçerli tüm <paramref name="ids"/> ürünlerini bir seferde yükler; eksikse handler NotFound fırlatır.</summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>Stok, indirim veya fiyat gibi toplu güncellemeleri yazar (sipariş oluşturma, admin vb.).</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
