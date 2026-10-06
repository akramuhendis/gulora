using FluentValidation;

namespace Gulora.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(300);

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug boş olamaz.")
            .MaximumLength(300)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug yalnızca küçük harf, rakam ve tire içerebilir.");

        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
