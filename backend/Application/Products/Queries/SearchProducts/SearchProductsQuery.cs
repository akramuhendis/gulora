using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.SearchProducts;

/// <summary>
/// Otocomplete, arama kutusu veya hızlı öneri senaryolarında kullanılmak üzere metin araması ile
/// sınırlı sayıda ürün kaydı döndüren MediatR sorgusu.
/// </summary>
/// <remarks>
/// Sonuçlar tam sayfalama yerine üst sınırlı (limit) liste olarak gelir; API katmanı bu sorguyu
/// düşük gecikmeli öneri uçlarına bağlayabilir.
/// </remarks>
/// <param name="SearchTerm">
/// Kullanıcının girdiği arama ifadesi; depo katmanında ürün adı, açıklama veya ilgili alanlarda eşleşme için kullanılır.
/// </param>
/// <param name="Limit">
/// Döndürülecek en fazla kayıt sayısı (varsayılan 10). Doğrulayıcı ile makul üst sınıra kısıtlanır.
/// </param>
public sealed record SearchProductsQuery(string SearchTerm, int Limit = 10) : IRequest<IReadOnlyList<ProductDto>>;
