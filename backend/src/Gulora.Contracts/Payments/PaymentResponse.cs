namespace Gulora.Contracts.Payments;

/// <summary>
/// Ödeme kaydı yanıtı.
/// Hassas kart/CVV bilgisi kesinlikle yer almaz (PCI-DSS).
/// Status: Pending | Completed | Failed | Refunded | PartiallyRefunded.
/// </summary>
public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    Guid UserId,
    string Method,
    string Status,
    decimal Amount,
    string Currency,
    string? ProviderTransactionId,
    DateTime? PaidAt,
    DateTime? RefundedAt,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
