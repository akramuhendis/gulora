using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Commands.RequestReturn;

public sealed class RequestReturnCommandHandler : IRequestHandler<RequestReturnCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestReturnCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RequestReturnCommand cmd, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException("Sipariş", cmd.OrderId);

        if (order.UserId != cmd.UserId)
            throw new ForbiddenException("Bu sipariş için iade talebi açma yetkiniz yok.");

        order.RequestReturn(cmd.Reason, cmd.UserId);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
