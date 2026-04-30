using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.SearchProducts;

/// <summary>
/// <see cref="SearchProductsQuery"/> isteğini işleyerek arama terimine göre ürünleri getirir ve her bir
/// <see cref="ProductDto"/> kaydına kategori slug ile görünen ad bilgisini ekler.
/// </summary>
/// <remarks>
/// Ürün varlığında kategori adı/slug tutulmadığından, N+1 sorgu oluşmaması için tüm kategoriler tek seferde
/// yüklenir ve kimlik sözlüğü ile eşlenir.
/// </remarks>
public sealed class SearchProductsQueryHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IMapper mapper) : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    /// <summary>
    /// Arama sonuçlarını getirir, boş liste durumunda erken döner; aksi halde kategori zenginleştirmesi yapar.
    /// </summary>
    /// <param name="request">Arama metni ve sonuç üst sınırını içeren sorgu.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Kategori bilgisi tamamlanmış ürün DTO listesi.</returns>
    public async Task<IReadOnlyList<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        // Depo: metin araması ve limit ile eşleşen ürün varlıkları
        var items = await products.SearchAsync(request.SearchTerm, request.Limit, cancellationToken).ConfigureAwait(false);
        if (items.Count == 0)
            return []; // Eşleşme yoksa boş dizi; kategori sorgusu yapılmaz

        // Liste üzerinde kategori eşlemesi için tüm kategorileri tek seferde yükle (performans)
        var allCategories = await categories.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var catById = allCategories.ToDictionary(c => c.Id);

        var dtos = new List<ProductDto>(items.Count);
        foreach (var p in items)
        {
            var dto = mapper.Map<ProductDto>(p);
            // Kategori silinmiş veya taşınmışsa DTO'da boş metin; istemci tutarlı davranır
            if (catById.TryGetValue(p.CategoryId, out var cat))
                dtos.Add(dto with { CategorySlug = cat.Slug, CategoryName = cat.Name });
            else
                dtos.Add(dto with { CategorySlug = string.Empty, CategoryName = string.Empty });
        }

        return dtos;
    }
}
