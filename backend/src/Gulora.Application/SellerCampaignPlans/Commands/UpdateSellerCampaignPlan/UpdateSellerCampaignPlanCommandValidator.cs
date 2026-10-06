using FluentValidation;

namespace Gulora.Application.SellerCampaignPlans.Commands.UpdateSellerCampaignPlan;

public sealed class UpdateSellerCampaignPlanCommandValidator : AbstractValidator<UpdateSellerCampaignPlanCommand>
{
    public UpdateSellerCampaignPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Kind)
            .NotEmpty()
            .Must(k => k.Equals("discount", StringComparison.OrdinalIgnoreCase)
                       || k.Equals("shipping", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Növ 'discount' və ya 'shipping' olmalıdır.");
        RuleFor(x => x.Percent).InclusiveBetween(1, 100);
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes != null);
        When(
            x => x.MinCartAmount.HasValue,
            () => RuleFor(x => x.MinCartAmount!.Value).GreaterThanOrEqualTo(0));
    }
}
