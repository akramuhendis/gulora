namespace Gulora.Domain.Events;

/// <summary>Yeni sipariş aggregate oluşturulduğunda yayınlanır.</summary>
public sealed record OrderCreatedDomainEvent(Guid OrderId) : DomainEventBase;
