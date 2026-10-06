using FluentValidation;

namespace Gulora.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(300).WithMessage("Ürün adı en fazla 300 karakter olabilir.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug boş olamaz.")
            .MaximumLength(300)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug yalnızca küçük harf, rakam ve tire içerebilir.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçilmelidir.");

        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).WithMessage("İndirimli fiyat, normal fiyattan küçük olmalıdır.")
            .When(x => x.DiscountedPrice.HasValue);
    }
}
