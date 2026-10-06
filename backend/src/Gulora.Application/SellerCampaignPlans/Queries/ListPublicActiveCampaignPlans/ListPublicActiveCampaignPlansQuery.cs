using Gulora.Contracts.SellerCampaigns;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Queries.ListPublicActiveCampaignPlans;

public sealed record ListPublicActiveCampaignPlansQuery
    : IRequest<IReadOnlyList<SellerCampaignPlanPublicHomeResponse>>;
