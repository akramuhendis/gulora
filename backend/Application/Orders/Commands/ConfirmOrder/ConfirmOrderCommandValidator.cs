using FluentValidation;

namespace Gulora.Application.Orders.Commands.ConfirmOrder;

/// <summary>
/// <see cref="ConfirmOrderCommand"/> için giriş doğrulaması. Yalnızca sipariş kimliğinin geçerli olmasını şart koşar.
/// </summary>
public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    /// <summary>
    /// <see cref="ConfirmOrderCommand.OrderId"/> alanına boş olmama kuralını ekler.
    /// </summary>
    public ConfirmOrderCommandValidator()
    {
        // Onay hedefi belirsiz istekleri reddet: sipariş kimliği zorunludur.
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
