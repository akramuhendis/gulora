using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Categories;
using MediatR;

namespace Gulora.Application.Categories.Queries.GetCategories;

/// <summary>
/// Tüm kategori varlıklarını depodan okur ve <see cref="CategoryDto"/> listesine AutoMapper ile eşler.
/// </summary>
/// <remarks>
/// Okuma ağırlıklı uç noktalar için basit akış: tek depo çağrısı ve projeksiyon; önbellekleme API katmanında yapılabilir.
/// </remarks>
public sealed class GetCategoriesQueryHandler(
    ICategoryRepository categories,
    IMapper mapper) : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    /// <summary>
    /// Kategorileri getirir ve DTO koleksiyonuna dönüştürür.
    /// </summary>
    /// <param name="request">Parametresiz sorgu (gelecekte filtre eklenebilir).</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Sözleşme tipinde salt okunur kategori listesi.</returns>
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Tüm kategorileri tek sorguda yükle
        var result = await categories.GetAllAsync(cancellationToken).ConfigureAwait(false);
        // Varlık → DTO eşlemesi merkezi profil üzerinden
        return mapper.Map<IReadOnlyList<CategoryDto>>(result);
    }
}
