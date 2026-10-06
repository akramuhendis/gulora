using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateOrderStatusCommand cmd, CancellationToken ct)
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
                    "Bu siparişte sipariş durumunu güncelleme yetkiniz yok. Sipariş başka satıcıların ürünlerini içeriyorsa bu işlem yalnızca yönetici tarafından yapılabilir.");
        }

        switch (cmd.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm(cmd.AdminUserId);
                break;
            case OrderStatus.Processing:
                order.StartProcessing(cmd.AdminUserId);
                break;
            case OrderStatus.Delivered:
                order.MarkDelivered(cmd.AdminUserId);
                break;
            case OrderStatus.Returned:
                order.CompleteReturn(cmd.AdminUserId);
                break;
            default:
                throw new BusinessRuleException(
                    $"'{cmd.NewStatus}' durumuna bu komutla geçilemez.");
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
