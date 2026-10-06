using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetAdminOrders;

/// <summary>Admin paneli için tüm siparişleri sayfalı getirir. Durum, kullanıcı ve tarih aralığı filtresi desteklenir.</summary>
public sealed record GetAdminOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    OrderStatus? Status = null,
    Guid? UserId = null,
    string? OrderNumber = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<PagedResult<OrderSummaryResponse>>;
