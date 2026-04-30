using FluentValidation;

namespace Gulora.Application.Orders.Commands.CompleteOrderReturn;

/// <summary>
/// <see cref="CompleteOrderReturnCommand"/> için FluentValidation kuralları; sipariş kimliği doğrulaması yapar.
/// </summary>
public sealed class CompleteOrderReturnCommandValidator : AbstractValidator<CompleteOrderReturnCommand>
{
    /// <summary>
    /// Sipariş kimliğinin boş olmaması kuralını ekler.
    /// </summary>
    public CompleteOrderReturnCommandValidator()
    {
        // İade tamamlama hedefi zorunludur.
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
