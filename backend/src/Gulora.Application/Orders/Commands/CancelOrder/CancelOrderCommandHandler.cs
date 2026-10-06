using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException("Sipariş", cmd.OrderId);

        if (!cmd.IsAdmin && order.UserId != cmd.RequestedByUserId)
            throw new ForbiddenException("Bu siparişi iptal etme yetkiniz yok.");

        order.Cancel(cmd.Reason, cmd.RequestedByUserId, allowAfterSellerConfirm: cmd.IsAdmin);

        // Stok iadesi
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            product?.IncrementStock(item.Quantity);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
