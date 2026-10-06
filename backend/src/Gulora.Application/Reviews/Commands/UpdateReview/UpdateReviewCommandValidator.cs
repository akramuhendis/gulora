using FluentValidation;

namespace Gulora.Application.Reviews.Commands.UpdateReview;

public sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 arasında olmalıdır.");
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null)
            .WithMessage("Başlık en fazla 200 karakter olabilir.");
        RuleFor(x => x.Body)
            .MaximumLength(2000)
            .When(x => x.Body is not null)
            .WithMessage("Yorum metni en fazla 2000 karakter olabilir.");
    }
}
