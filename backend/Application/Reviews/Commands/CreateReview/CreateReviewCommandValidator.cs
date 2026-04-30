using FluentValidation;

namespace Gulora.Application.Reviews.Commands.CreateReview;

/// <summary>
/// <see cref="CreateReviewCommand"/> alanları için FluentValidation ile biçim ve aralık doğrulamaları.
/// </summary>
/// <remarks>
/// İş kuralları (ürün varlığı, mükerrer yorum) komut işleyicide; burada yalnızca taşıma doğruluğu kontrol edilir.
/// </remarks>
public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    /// <summary>
    /// Ürün ve kullanıcı kimlikleri, puan aralığı ve yorum uzunluğu kurallarını ekler.
    /// </summary>
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        // Yıldız skalası 1–5 (inclusive)
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        // Boş yorum ve aşırı uzun metin engeli
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000);
    }
}
