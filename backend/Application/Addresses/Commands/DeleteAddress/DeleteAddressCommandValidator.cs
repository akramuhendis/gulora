using FluentValidation;

namespace Gulora.Application.Addresses.Commands.DeleteAddress;

/// <summary>
/// <see cref="DeleteAddressCommand"/> için FluentValidation kuralları: adres ve kullanıcı kimlikleri zorunludur.
/// </summary>
public sealed class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    /// <summary>
    /// Doğrulama kurallarını tanımlar: <c>AddressId</c> ve <c>UserId</c> boş olamaz.
    /// </summary>
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
