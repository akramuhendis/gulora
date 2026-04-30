using FluentValidation;

namespace Gulora.Application.Addresses.Commands.CreateAddress;

/// <summary>
/// <see cref="CreateAddressCommand"/> için FluentValidation kuralları: kimlik ve metin alanları zorunlu, uzunluk sınırları uygulanır.
/// </summary>
public sealed class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    /// <summary>
    /// Kuralları tanımlar: kullanıcı kimliği; başlık, alıcı, şehir, ilçe, satır zorunlu ve maksimum uzunluklar; posta kodu doluysa uzunluk sınırı.
    /// </summary>
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(32);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Line).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode is not null);
    }
}
