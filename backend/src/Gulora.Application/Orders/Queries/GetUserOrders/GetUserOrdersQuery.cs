using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetUserOrders;

/// <summary>Kullanıcıya ait siparişleri sayfalı getirir.</summary>
public sealed record GetUserOrdersQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderSummaryResponse>>;
