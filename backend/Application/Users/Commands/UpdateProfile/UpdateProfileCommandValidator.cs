using FluentValidation;

namespace Gulora.Application.Users.Commands.UpdateProfile;

/// <summary>
/// <see cref="UpdateProfileCommand"/> için FluentValidation: ad, soyad ve e-posta biçimi/uzunluk kuralları.
/// </summary>
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    /// <summary>
    /// Kuralları tanımlar: kullanıcı kimliği dolu; ad ve soyad zorunlu ve uzunluk sınırı; e-posta zorunlu, geçerli biçim ve uzunluk.
    /// </summary>
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
    }
}
