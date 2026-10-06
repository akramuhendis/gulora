using Gulora.Application.Common.Extensions;

using Gulora.Contracts.Common;

using Gulora.Contracts.Orders;

using Gulora.Domain.Interfaces;

using MediatR;



namespace Gulora.Application.Orders.Queries.GetUserOrders;



public sealed class GetUserOrdersQueryHandler

    : IRequestHandler<GetUserOrdersQuery, PagedResult<OrderSummaryResponse>>

{

    private readonly IOrderRepository _orderRepository;



    public GetUserOrdersQueryHandler(IOrderRepository orderRepository)

        => _orderRepository = orderRepository;



    public async Task<PagedResult<OrderSummaryResponse>> Handle(

        GetUserOrdersQuery query, CancellationToken ct)

    {

        var pageSize = Math.Min(query.PageSize, 50);

        var page = Math.Max(query.Page, 1);



        var orders = await _orderRepository.GetByUserIdPagedAsync(query.UserId, page, pageSize, ct);

        var totalCount = await _orderRepository.GetCountByUserIdAsync(query.UserId, ct);



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

            o.CreatedAt)).ToList();



        return new PagedResult<OrderSummaryResponse>(items, totalCount, page, pageSize);

    }

}

