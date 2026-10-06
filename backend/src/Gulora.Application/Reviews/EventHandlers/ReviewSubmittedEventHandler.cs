using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Reviews.EventHandlers;

public sealed class ReviewSubmittedEventHandler
    : INotificationHandler<DomainEventNotification<ReviewSubmittedEvent>>
{
    private readonly ILogger<ReviewSubmittedEventHandler> _logger;

    public ReviewSubmittedEventHandler(ILogger<ReviewSubmittedEventHandler> logger)
        => _logger = logger;

    public Task Handle(DomainEventNotification<ReviewSubmittedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation(
            "Yeni yorum moderasyon kuyruğunda. ReviewId: {ReviewId}, ProductId: {ProductId}, Rating: {Rating}",
            evt.ReviewId, evt.ProductId, evt.Rating);
        return Task.CompletedTask;
    }
}
