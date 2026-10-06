using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Commands.CompleteReturn;

public sealed class CompleteReturnCommandHandler : IRequestHandler<CompleteReturnCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteReturnCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompleteReturnCommand cmd, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new Gulora.Domain.Exceptions.NotFoundException("Sipariş", cmd.OrderId);

        if (cmd.SellerScopeUserId.HasValue)
        {
            if (!await _orderRepository.OrderInvolvesSellerAsync(
                    cmd.OrderId,
                    cmd.SellerScopeUserId.Value,
                    ct))
                throw new Gulora.Domain.Exceptions.ForbiddenException(
                    "Bu siparişte iade işlemini tamamlama yetkiniz yok.");
        }

        order.CompleteReturn(cmd.AdminUserId, cmd.Note);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
