using Gulora.Contracts.SellerCampaigns;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Queries.ListPublicSellerCampaignPlansBySlug;

public sealed record ListPublicSellerCampaignPlansBySlugQuery(string Slug)
    : IRequest<IReadOnlyList<SellerCampaignPlanPublicResponse>>;
