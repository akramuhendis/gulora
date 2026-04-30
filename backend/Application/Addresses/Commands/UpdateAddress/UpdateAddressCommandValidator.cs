using FluentValidation;

namespace Gulora.Application.Addresses.Commands.UpdateAddress;

/// <summary>
/// <see cref="UpdateAddressCommand"/> için FluentValidation kuralları: kullanıcı ve adres kimlikleri ile metin alanları doğrulanır.
/// </summary>
public sealed class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    /// <summary>
    /// Kuralları tanımlar: kimlikler boş olamaz; metin alanları zorunlu ve uzunluk sınırları; isteğe bağlı posta kodu uzunluğu.
    /// </summary>
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Line).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode is not null);
    }
}
