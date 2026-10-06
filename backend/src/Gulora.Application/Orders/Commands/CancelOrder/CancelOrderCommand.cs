using MediatR;

namespace Gulora.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Siparişi iptal eder. Müştəri: yalnız Pending; Admin: Pending və ya Confirmed.
/// Kaynak düzeyi yetki kontrolü: sipariş sahibi veya Admin.
/// </summary>
public sealed record CancelOrderCommand(
    Guid OrderId,
    Guid RequestedByUserId,
    string Reason,
    bool IsAdmin = false
) : IRequest;
