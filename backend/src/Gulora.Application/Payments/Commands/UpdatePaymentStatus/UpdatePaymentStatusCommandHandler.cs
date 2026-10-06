using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Payments.Commands.UpdatePaymentStatus;

public sealed class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentStatusCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePaymentStatusCommand cmd, CancellationToken ct)
    {
        var payment = await _paymentRepository.GetByIdempotencyKeyAsync(cmd.IdempotencyKey, ct)
            ?? throw new NotFoundException("Ödeme kaydı bulunamadı.");

        switch (cmd.NewStatus)
        {
            case PaymentStatus.Completed:
                payment.MarkCompleted(cmd.ProviderTransactionId);
                var order = await _orderRepository.GetByIdAsync(payment.OrderId, ct);
                order?.UpdatePaymentStatus(PaymentStatus.Completed);
                break;

            case PaymentStatus.Failed:
                payment.MarkFailed(cmd.FailureReason ?? "Ödeme başarısız.");
                var failedOrder = await _orderRepository.GetByIdAsync(payment.OrderId, ct);
                failedOrder?.UpdatePaymentStatus(PaymentStatus.Failed);
                break;

            case PaymentStatus.Refunded:
                payment.Refund();
                break;

            case PaymentStatus.PartiallyRefunded:
                payment.PartialRefund();
                break;

            default:
                throw new BusinessRuleException($"'{cmd.NewStatus}' durumuna geçiş desteklenmiyor.");
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
