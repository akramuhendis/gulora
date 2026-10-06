using FluentValidation;

namespace Gulora.Application.Orders.Commands.ShipOrder;

public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CargoCompany)
            .NotEmpty().WithMessage("Kargo firması boş olamaz.")
            .MaximumLength(100);
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("Takip numarası boş olamaz.")
            .MaximumLength(100);
    }
}
