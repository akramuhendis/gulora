using Gulora.Domain.Common;

namespace Gulora.Domain.Events;

/// <summary>
/// Sipariş iptal edildiğinde yayınlanır.
/// Handler'lar: stok iadesi, ödeme iadesi başlatma, kullanıcı bildirimi.
/// </summary>
public sealed record OrderCancelledEvent(
    Guid OrderId,
    Guid UserId,
    string OrderNumber,
    string? CancellationReason
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
