using MediatR;

namespace Gulora.Application.Orders.Commands.ShipOrder;

/// <summary>Siparişi kargoya verir (Processing → Shipped). Admin veya siparişte yalnızca kendi ürünleri olan satıcı.</summary>
public sealed record ShipOrderCommand(
    Guid OrderId,
    Guid AdminUserId,
    string CargoCompany,
    string TrackingNumber,
    Guid? SellerOperatorId = null
) : IRequest;
