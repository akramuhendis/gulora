using FluentValidation;

namespace Gulora.Application.Categories.Queries.GetCategories;

/// <summary>
/// <see cref="GetCategoriesQuery"/> için doğrulayıcı; şu an ek alan taşımadığından kurallar boştur.
/// </summary>
/// <remarks>
/// MediatR pipeline'ında diğer sorgularla tutarlı davranış için yer tutucu; ileride dil, mağaza veya aktiflik
/// filtresi eklendiğinde kurallar buraya taşınabilir.
/// </remarks>
public sealed class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
{
    /// <summary>
    /// Şimdilik kural eklenmez; genişletmeye açık yapı.
    /// </summary>
    public GetCategoriesQueryValidator()
    {
        // İsteğe bağlı: RuleFor ile gelecekteki alanlar burada doğrulanacak
    }
}
