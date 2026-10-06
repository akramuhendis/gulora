using FluentValidation;

namespace Gulora.Application.Products.Commands.ActivateProduct;

public sealed class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün kimliği boş olamaz.");
    }
}
