using Gulora.Domain.Common;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Events;

/// <summary>
/// Sipariş durumu değiştiğinde yayınlanır.
/// Handler'lar: kullanıcıya bildirim, kargo takip güncellemesi, istatistik.
/// </summary>
public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    Guid UserId,
    string OrderNumber,
    OrderStatus OldStatus,
    OrderStatus NewStatus
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
