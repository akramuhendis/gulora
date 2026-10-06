using FluentValidation;

namespace Gulora.Application.SellerApplications.Commands.SubmitSellerApplication;

public sealed class SubmitSellerApplicationCommandValidator
    : AbstractValidator<SubmitSellerApplicationCommand>
{
    public SubmitSellerApplicationCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Company).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TaxNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Message).MaximumLength(4000).When(x => x.Message is not null);
    }
}
