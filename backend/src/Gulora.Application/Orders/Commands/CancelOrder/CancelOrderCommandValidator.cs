using FluentValidation;

namespace Gulora.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Sipariş kimliği boş olamaz.");
        RuleFor(x => x.RequestedByUserId).NotEmpty().WithMessage("İptal eden kullanıcı kimliği boş olamaz.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("İptal gerekçesi boş olamaz.")
            .MaximumLength(500).WithMessage("İptal gerekçesi en fazla 500 karakter olabilir.");
    }
}
