using Gulora.Contracts.Categories;
using MediatR;

namespace Gulora.Application.Categories.Queries.GetCategories;

/// <summary>
/// Mağaza navigasyonu ve filtreler için tüm ürün kategorilerini parametresiz olarak listelemek üzere kullanılan sorgu.
/// </summary>
/// <remarks>
/// Şu an ek filtre taşımaz; kategori ağacı veya sıralama depo ve eşleme katmanında tanımlanır.
/// </remarks>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
