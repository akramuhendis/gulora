using FluentValidation;

namespace Gulora.Application.Users.Commands.AddAddress;

public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullLine).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode is not null);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2).WithMessage("Ülke kodu 2 karakter olmalıdır.");
    }
}
