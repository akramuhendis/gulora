using FluentValidation;

namespace Gulora.Application.Users.Commands.DeleteAddress;

public sealed class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Adres kimliği boş olamaz.");
    }
}
