using Gulora.Application.SellerCampaignPlans;
using Gulora.Application.SellerShops.Queries.GetSellerShopBySlug;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Queries.ListPublicSellerCampaignPlansBySlug;

public sealed class ListPublicSellerCampaignPlansBySlugQueryHandler
    : IRequestHandler<ListPublicSellerCampaignPlansBySlugQuery, IReadOnlyList<SellerCampaignPlanPublicResponse>>
{
    private readonly ISender _mediator;
    private readonly ISellerCampaignPlanRepository _campaignPlans;

    public ListPublicSellerCampaignPlansBySlugQueryHandler(
        ISender mediator,
        ISellerCampaignPlanRepository campaignPlans)
    {
        _mediator = mediator;
        _campaignPlans = campaignPlans;
    }

    public async Task<IReadOnlyList<SellerCampaignPlanPublicResponse>> Handle(
        ListPublicSellerCampaignPlansBySlugQuery query,
        CancellationToken ct)
    {
        var shop = await _mediator.Send(new GetSellerShopBySlugQuery(query.Slug), ct);
        var now = DateTime.UtcNow;
        var rows = await _campaignPlans.ListActiveBySellerAsync(shop.SellerUserId, now, ct);
        return rows.Select(SellerCampaignPlanMapper.ToPublicResponse).ToList();
    }
}
