using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Payments.Commands.UpdatePaymentStatus;

/// <summary>
/// Ödeme sağlayıcısı webhook'u sonrasında ödeme durumunu günceller.
/// İdempotency key çift işlemi önler (gulora-infrastructure-contracts.mdc).
/// </summary>
public sealed record UpdatePaymentStatusCommand(
    string IdempotencyKey,
    PaymentStatus NewStatus,
    string? ProviderTransactionId = null,
    string? FailureReason = null
) : IRequest;
