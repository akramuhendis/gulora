using FluentValidation;

namespace Gulora.Application.Payments.Commands.UpdatePaymentStatus;

public sealed class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("İdempotency anahtarı boş olamaz.")
            .MaximumLength(200);

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Geçersiz ödeme durumu.");

        RuleFor(x => x.ProviderTransactionId)
            .MaximumLength(200)
            .When(x => x.ProviderTransactionId is not null);

        RuleFor(x => x.FailureReason)
            .MaximumLength(500)
            .When(x => x.FailureReason is not null);
    }
}
