using FluentValidation;

namespace Gulora.Application.Reviews.Commands.ModerateReview;

public sealed class ModerateReviewCommandValidator : AbstractValidator<ModerateReviewCommand>
{
    public ModerateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .MaximumLength(1000)
            .When(x => !x.Approve);
    }
}
