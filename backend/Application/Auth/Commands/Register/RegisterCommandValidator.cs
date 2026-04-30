using FluentValidation;

namespace Gulora.Application.Auth.Commands.Register;

/// <summary>
/// <see cref="RegisterCommand"/> için FluentValidation: ad, soyad, e-posta, parola ve isteğe bağlı telefon kuralları.
/// </summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Kuralları tanımlar: ad/soyad zorunlu ve uzunluk sınırı; e-posta zorunlu, biçim ve uzunluk; telefon doluysa uzunluk; parola uzunluk aralığı.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Phone).MaximumLength(32).When(x => x.Phone is not null);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
