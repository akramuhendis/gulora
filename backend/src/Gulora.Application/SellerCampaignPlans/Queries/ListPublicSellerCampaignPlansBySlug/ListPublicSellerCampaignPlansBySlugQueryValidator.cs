using FluentValidation;

namespace Gulora.Application.SellerCampaignPlans.Queries.ListPublicSellerCampaignPlansBySlug;

public sealed class ListPublicSellerCampaignPlansBySlugQueryValidator
    : AbstractValidator<ListPublicSellerCampaignPlansBySlugQuery>
{
    public ListPublicSellerCampaignPlansBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
    }
}
