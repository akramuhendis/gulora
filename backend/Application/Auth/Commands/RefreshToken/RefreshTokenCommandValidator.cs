using FluentValidation;

namespace Gulora.Application.Auth.Commands.RefreshToken;

/// <summary>
/// <see cref="RefreshTokenCommand"/> için FluentValidation: yenileme jetonu boş olamaz.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// <c>RefreshToken</c> alanının dolu olmasını zorunlu kılar.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
