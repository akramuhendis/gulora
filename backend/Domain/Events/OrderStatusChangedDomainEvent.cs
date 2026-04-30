using Gulora.Domain.Enums;

namespace Gulora.Domain.Events;

/// <summary>Sipariş iş akışı durumu değiştiğinde yayınlanır.</summary>
public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    OrderStatus PreviousStatus,
    OrderStatus NewStatus) : DomainEventBase;
