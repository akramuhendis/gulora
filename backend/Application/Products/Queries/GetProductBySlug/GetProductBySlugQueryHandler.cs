using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Products;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProductBySlug;

/// <summary>
/// Slug ile tek ürün yükler; bulunamazsa <see cref="NotFoundException"/> üretir.
/// Dönen <see cref="ProductDto"/> üzerine kategori slug ve adı yansıtılır.
/// </summary>
/// <remarks>
/// Kategori bilgisi ayrı depo çağrısı ile alınır; tek ürün senaryosunda toplu kategori yüküne gerek yoktur.
/// </remarks>
public sealed class GetProductBySlugQueryHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IMapper mapper) : IRequestHandler<GetProductBySlugQuery, ProductDto>
{
    /// <summary>
    /// Slug'a göre ürünü arar, eşleşmezse hata fırlatır; eşleşirse DTO'yu kategori ile tamamlar.
    /// </summary>
    /// <param name="request">Ürün slug'ını içeren sorgu.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Kategori alanları doldurulmuş ürün DTO'su.</returns>
    /// <exception cref="NotFoundException">Slug ile ürün bulunamadığında.</exception>
    public async Task<ProductDto> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var product = await products.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (product is null)
            throw new NotFoundException("Product", request.Slug); // 404 semantiği için alan adı + aranan değer

        var dto = mapper.Map<ProductDto>(product);

        // Ürünün bağlı olduğu kategoriyi ayrı sorgula ve DTO'ya slug/ad olarak yansıt
        var category = await categories.GetByIdAsync(product.CategoryId, cancellationToken).ConfigureAwait(false);
        if (category is not null)
            return dto with { CategorySlug = category.Slug, CategoryName = category.Name };

        // Orphan veya tutarsız veri: istemciye boş kategori metinleri
        return dto with { CategorySlug = string.Empty, CategoryName = string.Empty };
    }
}
