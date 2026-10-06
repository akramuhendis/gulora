using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Product aggregate'ine özgü sorgu ve arama operasyonları.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>URL dostu slug ile ürün getirir.</summary>
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Kategoriye ait aktif ürünleri sayfalı getirir.</summary>
    Task<IReadOnlyList<Product>> GetByCategoryPagedAsync(
        Guid categoryId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Metin araması — isim və təsvirdə (ILIKE); isteğe bağlı kateqoriya, qiymət, stok süzgəci.</summary>
    Task<IReadOnlyList<Product>> SearchAsync(
        string keyword,
        int page,
        int pageSize,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        CancellationToken ct = default);

    /// <summary>SearchAsync ilə eyni filtrlə ümumi say.</summary>
    Task<int> SearchCountAsync(
        string keyword,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null,
        CancellationToken ct = default);

    /// <summary>Tüm aktif ürünleri sayfali getirir (kategori filtresi olmaksızın).</summary>
    Task<IReadOnlyList<Product>> GetAllActivePagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Tüm aktif ürünlerin sayısını döner.</summary>
    Task<int> GetAllActiveCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Katalog listesi — filtre, sıralama ve sayfalama (yalnızca aktif ürünler; global query filter).
    /// </summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetCatalogPagedAsync(
        Guid? categoryId,
        Guid? sellerId,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        bool? inStock,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Kataloqda endirimli qiymət və ya aktiv satıcı kampaniyası olan namizəd məhsullar.
    /// Əhatə JSON-u SQL-də yoxlanmır; tətbiq qatında təklif həlli ilə süzülür.
    /// </summary>
    Task<IReadOnlyList<Product>> ListPromotionCandidatesAsync(
        Guid? categoryId,
        Guid? sellerId,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isFeatured,
        bool? inStock,
        int maxCandidates,
        CancellationToken ct = default);

    /// <summary>Aktif katalog ürünü sayısı — mağaza vitrini için.</summary>
    Task<int> CountActiveCatalogBySellerAsync(Guid sellerId, CancellationToken ct = default);

    /// <summary>Slug'ın başka bir ürüne ait olup olmadığını kontrol eder.</summary>
    Task<bool> IsSlugTakenAsync(string slug, Guid? excludeProductId = null, CancellationToken ct = default);

    /// <summary>Stok miktarını günceller (sipariş oluşturulurken atomik azaltma).</summary>
    Task DecrementStockAsync(Guid productId, int quantity, CancellationToken ct = default);

    /// <summary>Satıcı paneli — yalnızca bu satıcıya ait ürünleri sayfalar.</summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetBySellerPagedAsync(
        Guid sellerId,
        int page,
        int pageSize,
        string? query = null,
        bool? isActive = null,
        bool? inStock = null,
        string? sortBy = null,
        CancellationToken ct = default);

    /// <summary>Bu satıcıya aid ürün sayı (soft delete filtresi ignore — satıcı paneli).</summary>
    Task<int> CountBySellerAsync(Guid sellerId, CancellationToken ct = default);

    /// <summary>Aşağı stok (0 &lt; stok ≤ eşik) və stoksuz sayları.</summary>
    Task<(int LowStock, int OutOfStock)> CountSellerStockBucketsAsync(
        Guid sellerId,
        int lowStockThreshold,
        CancellationToken ct = default);

    /// <summary>Satıcıya aid məhsul ID-ləri üçün ad siyahısı (əhatə yoxlaması).</summary>
    Task<IReadOnlyList<(Guid Id, string Name)>> ListProductNamesForSellerAsync(
        Guid sellerId,
        IReadOnlyList<Guid> productIds,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, string?>> GetPrimaryImageUrlsByProductIdsAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken ct = default);
}
