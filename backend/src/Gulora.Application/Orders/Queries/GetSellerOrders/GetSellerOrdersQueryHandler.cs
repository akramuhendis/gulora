using Gulora.Application.Common.Extensions;
using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetSellerOrders;

public sealed class GetSellerOrdersQueryHandler
    : IRequestHandler<GetSellerOrdersQuery, PagedResult<OrderSummaryResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetSellerOrdersQueryHandler(IOrderRepository orderRepository)
        => _orderRepository = orderRepository;

    public async Task<PagedResult<OrderSummaryResponse>> Handle(
        GetSellerOrdersQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var (orders, totalCount) = await _orderRepository.GetPagedForSellerAsync(
            query.SellerUserId,
            query.Status,
            query.OrderNumber,
            query.DateFrom,
            query.DateTo,
            page,
            pageSize,
            ct);

        var attributed = await _orderRepository.GetSellerAttributedTotalsByOrderIdsAsync(
            orders.Select(o => o.Id).ToList(),
            query.SellerUserId,
            ct);

        var items = orders.Select(o => new OrderSummaryResponse(
            o.Id,
            o.OrderNumber,
            o.Status.ToString(),
            o.Status.ToDisplayString(),
            o.DeliveryType.ToString(),
            o.PaymentMethod.ToString(),
            o.PaymentStatus.ToString(),
            o.TotalAmount,
            o.Items.Count,
            o.Items.FirstOrDefault()?.ProductName,
            null,
            o.CanBeCancelled,
            o.CanBeReturned,
            o.CreatedAt,
            attributed.TryGetValue(o.Id, out var at) ? at : 0m)).ToList();

        return new PagedResult<OrderSummaryResponse>(items, totalCount, page, pageSize);
    }
}
