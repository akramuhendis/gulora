using Gulora.Application.Common.Extensions;
using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetAdminOrders;

public sealed class GetAdminOrdersQueryHandler
    : IRequestHandler<GetAdminOrdersQuery, PagedResult<OrderSummaryResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAdminOrdersQueryHandler(IOrderRepository orderRepository)
        => _orderRepository = orderRepository;

    public async Task<PagedResult<OrderSummaryResponse>> Handle(
        GetAdminOrdersQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        IReadOnlyList<Domain.Entities.Order> orders;

        if (query.Status.HasValue)
            orders = await _orderRepository.GetByStatusAsync(query.Status.Value, ct);
        else if (query.DateFrom.HasValue && query.DateTo.HasValue)
            orders = await _orderRepository.GetByDateRangeAsync(query.DateFrom.Value, query.DateTo.Value, ct);
        else if (query.UserId.HasValue)
            orders = await _orderRepository.GetByUserIdPagedAsync(query.UserId.Value, page, pageSize, ct);
        else
            orders = await _orderRepository.GetByDateRangeAsync(
                DateTime.UtcNow.AddDays(-90), DateTime.UtcNow, ct);

        // Sipariş numarası filtresi (bellek içi — Repository'de LIKE desteği olana kadar)
        if (!string.IsNullOrWhiteSpace(query.OrderNumber))
            orders = orders
                .Where(o => o.OrderNumber.Contains(query.OrderNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var totalCount = orders.Count;
        var paged = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = paged.Select(o => new OrderSummaryResponse(
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
