using FluentValidation;

namespace Gulora.Application.SellerShops.Commands.UpsertMySellerShop;

public sealed class UpsertMySellerShopCommandValidator : AbstractValidator<UpsertMySellerShopCommand>
{
    private static readonly System.Text.RegularExpressions.Regex SlugPattern =
        new("^[a-z0-9]+(?:-[a-z0-9]+)*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public UpsertMySellerShopCommandValidator()
    {
        RuleFor(x => x.StoreName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StoreSlug)
            .NotEmpty()
            .MaximumLength(120)
            .Must(s => SlugPattern.IsMatch(s.Trim().ToLowerInvariant()))
            .WithMessage(
                "Mağaza adresi yalnızca küçük harf, rakam ve tire içerebilir (ör. gul-bagi-vitrin).");
        RuleFor(x => x.Description).MaximumLength(8000);
        RuleFor(x => x.LogoUrl).MaximumLength(2000);
        RuleFor(x => x.BannerUrl).MaximumLength(2000);
    }
}
