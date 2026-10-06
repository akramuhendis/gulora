using Gulora.Application.Common.Notifications;
using Gulora.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Gulora.Infrastructure.Persistence.Interceptors;

/// <summary>
/// SaveChanges başarılı olduktan sonra varlıklarda toplanan domain olaylarını
/// MediatR üzerinden yayınlar.
/// gulora-cqrs-patterns.mdc: SaveChanges sonrası domain event dispatch.
/// </summary>
internal sealed class DomainEventSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventSaveChangesInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        if (eventData.Context is not null)
            await DispatchDomainEventsAsync(eventData.Context, ct);

        return await base.SavedChangesAsync(eventData, result, ct);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Olayları temizle — yeniden gönderimi önle
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await _publisher.Publish(notification, ct);
        }
    }
}
