using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Users.EventHandlers;

public sealed class UserPasswordChangedEventHandler
    : INotificationHandler<DomainEventNotification<UserPasswordChangedEvent>>
{
    private readonly ILogger<UserPasswordChangedEventHandler> _logger;

    public UserPasswordChangedEventHandler(ILogger<UserPasswordChangedEventHandler> logger)
        => _logger = logger;

    public Task Handle(DomainEventNotification<UserPasswordChangedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("Kullanıcı parolası değiştirildi. UserId: {UserId}", evt.UserId);
        return Task.CompletedTask;
    }
}
