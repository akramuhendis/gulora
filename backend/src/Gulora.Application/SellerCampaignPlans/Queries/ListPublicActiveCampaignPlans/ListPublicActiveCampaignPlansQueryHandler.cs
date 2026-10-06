using Gulora.Application.SellerCampaignPlans;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Queries.ListPublicActiveCampaignPlans;

public sealed class ListPublicActiveCampaignPlansQueryHandler
    : IRequestHandler<ListPublicActiveCampaignPlansQuery, IReadOnlyList<SellerCampaignPlanPublicHomeResponse>>
{
    private readonly ISellerCampaignPlanRepository _campaignPlans;

    public ListPublicActiveCampaignPlansQueryHandler(ISellerCampaignPlanRepository campaignPlans)
    {
        _campaignPlans = campaignPlans;
    }

    public async Task<IReadOnlyList<SellerCampaignPlanPublicHomeResponse>> Handle(
        ListPublicActiveCampaignPlansQuery request,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = await _campaignPlans.ListActiveWithShopSlugsAsync(now, ct);
        return rows
            .Select(r => SellerCampaignPlanMapper.ToPublicHomeResponse(r.Plan, r.StoreSlug, r.StoreName))
            .ToList();
    }
}
