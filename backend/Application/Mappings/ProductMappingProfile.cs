using AutoMapper;
using Gulora.Contracts.Products;
using Gulora.Domain.Entities;

namespace Gulora.Application.Mappings;

/// <summary>
/// <see cref="Product"/> (ürün) <c>Contracts</c> DTO’suna dönüştürme. Görsel/etiket alanları
/// IEnumerable olabilir; DTO’da <see cref="List{T}"/> için <c>ToList</c> ile somut listeye kopyalanır.
/// Kategori adı ve slug, query handler’da doldurulabildiğinden <c>Ignore</c> ile bırakılır.
/// </summary>
public sealed class ProductMappingProfile : Profile
{
    /// <summary>Liste/etiket alanlarını kopyala; kategori dış alanlar handler’dan doldurulacak.</summary>
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images.ToList()))
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.Tags.ToList()))
            .ForMember(d => d.CategorySlug, o => o.Ignore())
            .ForMember(d => d.CategoryName, o => o.Ignore());
    }
}
