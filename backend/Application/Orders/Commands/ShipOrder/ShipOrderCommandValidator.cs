using FluentValidation;

namespace Gulora.Application.Orders.Commands.ShipOrder;

/// <summary>
/// <see cref="ShipOrderCommand"/> için FluentValidation kuralları; komutun hedef sipariş kimliğini doğrular.
/// </summary>
public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    /// <summary>
    /// <see cref="ShipOrderCommand.OrderId"/> için boş olmama kuralını ekler.
    /// </summary>
    public ShipOrderCommandValidator()
    {
        // Sevk işlemi için geçerli sipariş kimliği zorunludur.
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
