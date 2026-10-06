using Gulora.Contracts.Orders;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetOrderById;

/// <summary>
/// Sipariş detayını getirir.
/// Yetki: sipariş sahibi, Admin veya siparişte ürünü olan Satıcı (<paramref name="SellerScopeUserId"/>).
/// </summary>
public sealed record GetOrderByIdQuery(
    Guid OrderId,
    Guid RequestedByUserId,
    bool IsAdmin = false,
    Guid? SellerScopeUserId = null
) : IRequest<OrderResponse>;
