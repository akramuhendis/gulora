namespace Gulora.Contracts.Payments;

/// <summary>
/// POST api/v1/payments/{id}/status gövdesi (Webhook / Admin).
/// ProviderTransactionId ödeme sağlayıcısından gelen referans ID'sidir; hassas değildir.
/// </summary>
public sealed record UpdatePaymentStatusRequest(
    string NewStatus,
    string? ProviderTransactionId = null,
    string? FailureReason = null);
