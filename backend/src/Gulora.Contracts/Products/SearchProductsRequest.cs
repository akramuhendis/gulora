using Gulora.Contracts.Common;

namespace Gulora.Contracts.Products;

/// <summary>GET api/v1/products/search sorgu parametreleri.</summary>
public sealed record SearchProductsRequest : PagedRequest
{
    /// <summary>Arama terimi (ürün adı, marka, açıklama).</summary>
    public string? Query { get; init; }

    /// <summary>Kategori kimliği ile daralt.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>Minimum fiyat.</summary>
    public decimal? MinPrice { get; init; }

    /// <summary>Maksimum fiyat.</summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>Yalnız stokda olan məhsullar (true).</summary>
    public bool? InStock { get; init; }
}
