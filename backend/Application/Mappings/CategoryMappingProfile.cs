using AutoMapper;
using Gulora.Contracts.Categories;
using Gulora.Domain.Entities;

namespace Gulora.Application.Mappings;

/// <summary>
/// <see cref="Category"/> katalog ağacı/ liste öğesi ile yönetim-veya-ön yüz
/// <see cref="CategoryDto"/> eşlemesi; ikon, sıra, slug alanları otomatik eşlenir.
/// </summary>
public sealed class CategoryMappingProfile : Profile
{
    /// <summary>Menü ve "tüm kategoriler" sorgu sonuçları bu profilde üretilir.</summary>
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>();
    }
}
