using FluentValidation;

namespace Gulora.Application.Corporate.Commands.SubmitInquiry;

public sealed class SubmitInquiryCommandValidator : AbstractValidator<SubmitInquiryCommand>
{
    public SubmitInquiryCommandValidator()
    {
        RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
        RuleFor(x => x.Budget).GreaterThan(0).When(x => x.Budget.HasValue);
    }
}
