using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Queries.ListSellerCampaignPlans;

public sealed record ListSellerCampaignPlansQuery : IRequest<IReadOnlyList<SellerCampaignPlanResponse>>;

public sealed class ListSellerCampaignPlansQueryHandler
    : IRequestHandler<ListSellerCampaignPlansQuery, IReadOnlyList<SellerCampaignPlanResponse>>
{
    private readonly ISellerCampaignPlanRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public ListSellerCampaignPlansQueryHandler(
        ISellerCampaignPlanRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SellerCampaignPlanResponse>> Handle(
        ListSellerCampaignPlansQuery _,
        CancellationToken ct)
    {
        var userId = SellerCampaignAuth.RequireSellerUserId(_currentUser);
        var rows = await _repository.ListBySellerAsync(userId, ct);
        return rows.Select(SellerCampaignPlanMapper.ToResponse).ToList();
    }
}
