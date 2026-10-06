using FluentValidation;

namespace Gulora.Application.Corporate.Commands.SubmitInquiry;

public sealed class SubmitCorporateInquiryCommandValidator : AbstractValidator<SubmitCorporateInquiryCommand>
{
    public SubmitCorporateInquiryCommandValidator()
    {
        RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Occasion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Quantity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Budget).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Message).MaximumLength(2000).When(x => x.Message is not null);
    }
}
