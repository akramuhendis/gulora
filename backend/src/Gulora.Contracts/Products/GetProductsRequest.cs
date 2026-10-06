using Gulora.Contracts.Common;

namespace Gulora.Contracts.Products;

/// <summary>
/// GET api/v1/products sorgu parametreleri — sayfalama + filtre + sıralama.
/// pageSize ≤ 100 (validation kuralı).
/// </summary>
public sealed record GetProductsRequest : PagedRequest
{
    /// <summary>Kategori kimliği ile filtrele.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>Yalnızca bu satıcı mağazasına ait ürünler (satıcı kullanıcı kimliği).</summary>
    public Guid? SellerId { get; init; }

    /// <summary>Marka ile filtrele.</summary>
    public string? Brand { get; init; }

    /// <summary>Minimum fiyat.</summary>
    public decimal? MinPrice { get; init; }

    /// <summary>Maksimum fiyat.</summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>Yalnızca öne çıkan ürünler.</summary>
    public bool? IsFeatured { get; init; }

    /// <summary>Yalnızca stokta olan ürünler.</summary>
    public bool? InStock { get; init; }

    /// <summary>
    /// Sıralama alanı: price_asc | price_desc | newest | rating | name_asc | name_desc.
    /// Varsayılan: newest.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Yalnız kataloq endirimi və ya aktiv satıcı kampanyası təklifi olan məhsullar.
    /// </summary>
    public bool? OnPromotion { get; init; }
}
