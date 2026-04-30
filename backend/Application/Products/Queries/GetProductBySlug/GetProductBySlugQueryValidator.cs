using FluentValidation;

namespace Gulora.Application.Products.Queries.GetProductBySlug;

/// <summary>
/// <see cref="GetProductBySlugQuery"/> için slug alanının varlığı ve uzunluk üst sınırı doğrulaması.
/// </summary>
/// <remarks>
/// Maksimum uzunluk, veritabanı kolonu ve URL güvenliği ile uyumlu tutulur.
/// </remarks>
public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    /// <summary>
    /// Slug için FluentValidation kurallarını kaydeder.
    /// </summary>
    public GetProductBySlugQueryValidator()
    {
        // Boş veya yalnızca boşluk slug ile gereksiz depo çağrısını önle
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(256);
    }
}
