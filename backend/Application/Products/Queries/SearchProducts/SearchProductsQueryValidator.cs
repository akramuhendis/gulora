using FluentValidation;

namespace Gulora.Application.Products.Queries.SearchProducts;

/// <summary>
/// <see cref="SearchProductsQuery"/> için FluentValidation kuralları: arama metni ve sonuç limiti.
/// </summary>
/// <remarks>
/// Limit üst sınırı, aşırı yük ve kötüye kullanımı önlemek için API ile uyumlu dar bir aralıkta tutulur.
/// </remarks>
public sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    /// <summary>
    /// Kuralları oluşturur: arama metni boş ve aşırı uzun olamaz; limit 1–50 aralığında olmalıdır.
    /// </summary>
    public SearchProductsQueryValidator()
    {
        // Anlamsız veya tamamen boş aramayı reddet; çok uzun string ile depo/URL yükünü sınırla
        RuleFor(x => x.SearchTerm).NotEmpty().MaximumLength(200);
        // Öneri/liste uçları için makul üst sınır (ör. 50'den fazla kayıt tek istekte dönmez)
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
    }
}
