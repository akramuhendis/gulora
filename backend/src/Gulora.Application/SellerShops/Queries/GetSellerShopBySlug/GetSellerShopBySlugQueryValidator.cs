using FluentValidation;

namespace Gulora.Application.SellerShops.Queries.GetSellerShopBySlug;

public sealed class GetSellerShopBySlugQueryValidator : AbstractValidator<GetSellerShopBySlugQuery>
{
    public GetSellerShopBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
    }
}
