using FluentValidation;

namespace Gulora.Application.Orders.Commands.CompleteReturn;

public sealed class CompleteReturnCommandValidator : AbstractValidator<CompleteReturnCommand>
{
    public CompleteReturnCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000).When(x => x.Note is not null);
    }
}
