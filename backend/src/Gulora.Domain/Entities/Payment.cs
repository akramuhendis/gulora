using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Ödeme işlemi kaydı — Order aggregate'i ile ilişkilidir.
/// Kart numarası, CVV gibi hassas veriler bu entity'de saklanmaz (PCI-DSS).
/// Ödeme sağlayıcısı referans kimliği (ProviderTransactionId) izlenebilirlik için tutulur.
/// </summary>
public sealed class Payment : BaseAggregateRoot
{
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "AZN";

    /// <summary>Ödeme sağlayıcısının döndürdüğü işlem ID'si (loglanabilir, hassas değil).</summary>
    public string? ProviderTransactionId { get; private set; }

    /// <summary>İdempotency anahtarı — çift tahsilatı önler.</summary>
    public string IdempotencyKey { get; private set; } = string.Empty;

    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid userId,
        PaymentMethod method,
        decimal amount,
        string idempotencyKey,
        string currency = "AZN")
    {
        if (amount <= 0)
            throw new BusinessRuleException("Ödeme tutarı sıfırdan büyük olmalıdır.");

        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        return new Payment
        {
            OrderId = orderId,
            UserId = userId,
            Method = method,
            Status = PaymentStatus.Pending,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            IdempotencyKey = idempotencyKey
        };
    }

    /// <summary>Ödeme başarılıyla tamamlandı (webhook sonrası).</summary>
    public void MarkCompleted(string? providerTransactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new BusinessRuleException("Yalnızca bekleyen ödemeler tamamlanabilir.");

        Status = PaymentStatus.Completed;
        ProviderTransactionId = providerTransactionId;
        PaidAt = DateTime.UtcNow;
        MarkUpdated();
    }

    /// <summary>Ödeme başarısız oldu.</summary>
    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason?.Trim();
        MarkUpdated();
    }

    /// <summary>Tam iade.</summary>
    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new BusinessRuleException("Yalnızca tamamlanmış ödemeler iade edilebilir.");

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    /// <summary>Kısmi iade.</summary>
    public void PartialRefund()
    {
        if (Status != PaymentStatus.Completed)
            throw new BusinessRuleException("Yalnızca tamamlanmış ödemeler iade edilebilir.");

        Status = PaymentStatus.PartiallyRefunded;
        RefundedAt = DateTime.UtcNow;
        MarkUpdated();
    }
}
