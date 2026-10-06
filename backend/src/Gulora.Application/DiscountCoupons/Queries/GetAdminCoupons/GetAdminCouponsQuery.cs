using Gulora.Contracts.Common;
using Gulora.Contracts.DiscountCoupons;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Queries.GetAdminCoupons;

public sealed record GetAdminCouponsQuery(int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<CouponResponse>>;

public sealed class GetAdminCouponsQueryHandler
    : IRequestHandler<GetAdminCouponsQuery, PagedResult<CouponResponse>>
{
    private readonly IDiscountCouponRepository _repository;

    public GetAdminCouponsQueryHandler(IDiscountCouponRepository repository)
        => _repository = repository;

    public async Task<PagedResult<CouponResponse>> Handle(
        GetAdminCouponsQuery query,
        CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var coupons = await _repository.ListPagedAsync(null, page, pageSize, ct);
        var total = await _repository.CountAsync(null, ct);

        var items = coupons.Select(c => new CouponResponse(
            c.Id,
            c.Code,
            c.Description,
            c.Type.ToString(),
            c.Value,
            c.MinOrderAmount,
            c.MaxDiscountAmount,
            c.UsageLimit,
            c.UsageCount,
            c.StartsAt,
            c.ExpiresAt,
            c.IsActive,
            c.RestrictedUserId,
            c.CreatedAt)).ToList();

        return new PagedResult<CouponResponse>(items, total, page, pageSize);
    }
}
