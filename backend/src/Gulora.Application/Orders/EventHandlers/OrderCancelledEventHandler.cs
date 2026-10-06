using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Orders.EventHandlers;

public sealed class OrderCancelledEventHandler
    : INotificationHandler<DomainEventNotification<OrderCancelledEvent>>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
        => _logger = logger;

    public Task Handle(DomainEventNotification<OrderCancelledEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation(
            "Sipariş iptal edildi. OrderNumber: {OrderNumber}, UserId: {UserId}, Reason: {Reason}",
            evt.OrderNumber, evt.UserId, evt.CancellationReason);
        return Task.CompletedTask;
    }
}
