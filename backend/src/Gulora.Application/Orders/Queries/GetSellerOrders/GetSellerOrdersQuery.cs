using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetSellerOrders;

/// <summary>Satıcı paneli — ürünlerinin yer aldığı siparişleri listeler.</summary>
public sealed record GetSellerOrdersQuery(
    Guid SellerUserId,
    int Page = 1,
    int PageSize = 20,
    OrderStatus? Status = null,
    string? OrderNumber = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<PagedResult<OrderSummaryResponse>>;
