using FluentValidation;

namespace Gulora.Application.Products.Commands.AddProductImage;

public sealed class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2000).Must(BeHttpOrHttps)
            .WithMessage("Geçerli bir http veya https URL'si girin.");
        RuleFor(x => x.AltText).MaximumLength(500).When(x => x.AltText is not null);
        RuleFor(x => x.DisplayOrder).InclusiveBetween(0, 999);
    }

    private static bool BeHttpOrHttps(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var u)
           && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}
