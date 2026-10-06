using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Sipariş durumunu yüksek seviyeli günceller (Admin akışı: Confirm, StartProcessing, MarkDelivered).
/// Kargo için <see cref="ShipOrder.ShipOrderCommand"/> kullanılır.
/// </summary>
public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    Guid AdminUserId,
    OrderStatus NewStatus,
    Guid? SellerOperatorId = null
) : IRequest;
