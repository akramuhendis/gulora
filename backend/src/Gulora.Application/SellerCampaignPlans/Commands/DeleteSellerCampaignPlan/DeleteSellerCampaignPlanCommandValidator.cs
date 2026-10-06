using FluentValidation;

namespace Gulora.Application.SellerCampaignPlans.Commands.DeleteSellerCampaignPlan;

public sealed class DeleteSellerCampaignPlanCommandValidator : AbstractValidator<DeleteSellerCampaignPlanCommand>
{
    public DeleteSellerCampaignPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
