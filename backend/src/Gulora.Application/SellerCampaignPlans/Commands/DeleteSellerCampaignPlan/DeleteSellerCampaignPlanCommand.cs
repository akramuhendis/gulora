using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Commands.DeleteSellerCampaignPlan;

public sealed record DeleteSellerCampaignPlanCommand(Guid Id) : IRequest<Unit>;
