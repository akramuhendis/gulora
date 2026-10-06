using FluentValidation;

namespace Gulora.Application.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut parola boş olamaz.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni parola boş olamaz.")
            .MinimumLength(8).WithMessage("Parola en az 8 karakter olmalıdır.")
            .Matches(@"[A-Z]").WithMessage("Parola en az bir büyük harf içermelidir.")
            .Matches(@"[0-9]").WithMessage("Parola en az bir rakam içermelidir.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni parola mevcut parola ile aynı olamaz.");
    }
}
