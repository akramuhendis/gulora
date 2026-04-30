using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProducts;

/// <summary>
/// <see cref="GetProductsQuery"/> isteğini işler: depodan sayfalı ürünleri çeker ve her DTO'ya kategori slug/ad ekler.
/// </summary>
/// <remarks>
/// Kategori adı ve slug değerleri ürün varlığında doğrudan tutulmadığı için tüm kategoriler yüklenip
/// kimlik sözlüğü ile eşlenir; böylece sayfadaki ürün sayısı kadar tekrarlayan kategori sorgusu oluşmaz.
/// </remarks>
public sealed class GetProductsQueryHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IMapper mapper) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    /// <summary>
    /// Filtre ve sayfalama ile ürünleri getirir, DTO'ları eşler ve sayfalı sonuç nesnesi döndürür.
    /// </summary>
    /// <param name="request"><see cref="GetProductsRequest"/> sarmalayan sorgu.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Ürün DTO listesi, toplam kayıt sayısı ve sayfa bilgisi.</returns>
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var r = request.Request;

        // Filtre ve sıralama ile sayfalı ürün kayıtlarını ve toplam kayıt sayısını getir
        var (items, total) = await products.GetPagedAsync(
            r.CategorySlug,
            r.MinPrice,
            r.MaxPrice,
            r.Tag,
            r.Badge,
            r.IsSameDay,
            r.IsFreeDelivery,
            r.SortBy,
            r.Page,
            r.PageSize,
            cancellationToken).ConfigureAwait(false);

        // Ürün DTO'larına kategori slug ve adını hızlı eşleştirmek için tüm kategorileri belleğe al
        var allCategories = await categories.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var catById = allCategories.ToDictionary(c => c.Id);

        var dtos = new List<ProductDto>(items.Count);
        foreach (var p in items)
        {
            var dto = mapper.Map<ProductDto>(p);
            // Kategori bulunursa DTO'yu kategori bilgisiyle güncelle; yoksa boş metin ata
            if (catById.TryGetValue(p.CategoryId, out var cat))
                dtos.Add(dto with { CategorySlug = cat.Slug, CategoryName = cat.Name });
            else
                dtos.Add(dto with { CategorySlug = string.Empty, CategoryName = string.Empty });
        }

        // İstemcinin sayfa navigasyonu için toplam ve istenen sayfa boyutu korunur
        return new PagedResult<ProductDto>(dtos, total, r.Page, r.PageSize);
    }
}
