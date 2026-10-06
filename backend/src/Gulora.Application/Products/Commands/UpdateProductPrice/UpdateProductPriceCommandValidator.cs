using FluentValidation;

namespace Gulora.Application.Products.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.NewPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");

        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.NewPrice).WithMessage("İndirimli fiyat, normal fiyattan küçük olmalıdır.")
            .When(x => x.DiscountedPrice.HasValue);
    }
}
