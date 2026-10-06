using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Products.EventHandlers;

public sealed class ProductStockUpdatedEventHandler
    : INotificationHandler<DomainEventNotification<ProductStockUpdatedEvent>>
{
    private readonly ILogger<ProductStockUpdatedEventHandler> _logger;

    public ProductStockUpdatedEventHandler(ILogger<ProductStockUpdatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(DomainEventNotification<ProductStockUpdatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation(
            "Ürün stoku güncellendi. ProductId: {ProductId}, Old: {OldStock}, New: {NewStock}",
            evt.ProductId, evt.OldStock, evt.NewStock);
        return Task.CompletedTask;
    }
}
