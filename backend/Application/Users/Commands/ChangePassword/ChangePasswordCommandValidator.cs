using FluentValidation;

namespace Gulora.Application.Users.Commands.ChangePassword;

/// <summary>
/// <see cref="ChangePasswordCommand"/> için FluentValidation: kimlik ve parola alanları zorunlu, yeni parola uzunluk aralığı.
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Kuralları tanımlar: kullanıcı kimliği ve mevcut parola boş olamaz; yeni parola minimum/maksimum uzunluk.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
