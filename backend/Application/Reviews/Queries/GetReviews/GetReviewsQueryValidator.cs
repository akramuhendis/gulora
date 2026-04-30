using FluentValidation;

namespace Gulora.Application.Reviews.Queries.GetReviews;

/// <summary>
/// <see cref="GetReviewsQuery"/> için ürün kimliği ve sayfalama parametrelerinin geçerliliğini doğrular.
/// </summary>
/// <remarks>
/// Boş <see cref="System.Guid"/> ve aşırı büyük sayfa boyutları hem güvenlik hem de performans açısından engellenir.
/// </remarks>
public sealed class GetReviewsQueryValidator : AbstractValidator<GetReviewsQuery>
{
    /// <summary>
    /// Ürün zorunlu; sayfa ve sayfa boyutu pozitif ve üst sınırlı olmalıdır.
    /// </summary>
    public GetReviewsQueryValidator()
    {
        // Varsayılan Guid ile tüm yorumları çekmeyi önle
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        // Tek yanıtta makul üst sınır
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
