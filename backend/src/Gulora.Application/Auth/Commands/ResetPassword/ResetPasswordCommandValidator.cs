using FluentValidation;

namespace Gulora.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Sıfırlama token'ı boş olamaz.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni parola boş olamaz.")
            .MinimumLength(8).WithMessage("Parola en az 8 karakter olmalıdır.")
            .Matches(@"[A-Z]").WithMessage("Parola en az bir büyük harf içermelidir.")
            .Matches(@"[0-9]").WithMessage("Parola en az bir rakam içermelidir.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Parola tekrarı boş olamaz.")
            .Equal(x => x.NewPassword).WithMessage("Parolalar eşleşmiyor.");
    }
}
