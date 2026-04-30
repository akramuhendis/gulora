using FluentValidation;

namespace Gulora.Application.Auth.Commands.Login;

/// <summary>
/// <see cref="LoginCommand"/> için FluentValidation: e-posta biçimi ve dolu parola zorunludur.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Kuralları tanımlar: e-posta boş olamaz ve geçerli biçimde olmalı; parola boş olamaz.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
