using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Commands.ShipOrder;

public sealed class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShipOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ShipOrderCommand cmd, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException("Sipariş", cmd.OrderId);

        if (cmd.SellerOperatorId.HasValue)
        {
            if (!await _orderRepository.OrderIsFullyOwnedBySellerAsync(
                    cmd.OrderId,
                    cmd.SellerOperatorId.Value,
                    ct))
                throw new ForbiddenException(
                    "Bu siparişi kargoya verme yetkiniz yok. Sipariş başka satıcıların ürünlerini içeriyorsa bu işlem yalnızca yönetici tarafından yapılabilir.");
        }

        order.Ship(cmd.CargoCompany, cmd.TrackingNumber, cmd.AdminUserId);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
