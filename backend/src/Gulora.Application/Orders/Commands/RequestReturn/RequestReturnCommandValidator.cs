using FluentValidation;

namespace Gulora.Application.Orders.Commands.RequestReturn;

public sealed class RequestReturnCommandValidator : AbstractValidator<RequestReturnCommand>
{
    public RequestReturnCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("İade gerekçesi boş olamaz.")
            .MaximumLength(500).WithMessage("İade gerekçesi en fazla 500 karakter olabilir.");
    }
}
