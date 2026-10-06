using Gulora.Application.Common.Interfaces;
using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Users.EventHandlers;

/// <summary>
/// Kullanıcı kaydolduğunda tetiklenir.
/// Şu anda e-posta doğrulama akışı RegisterCommandHandler'da yönetiliyor;
/// bu handler ek bildirim veya arka plan işleri için hazır tutulmaktadır.
/// </summary>
public sealed class UserRegisteredEventHandler
    : INotificationHandler<DomainEventNotification<UserRegisteredEvent>>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<UserRegisteredEvent> notification,
        CancellationToken ct)
    {
        var evt = notification.DomainEvent;

        // E-posta PII olduğundan loglanmaz (gulora-security.mdc).
        _logger.LogInformation(
            "Yeni kullanıcı kaydoldu. UserId: {UserId}, Role: {Role}",
            evt.UserId, evt.Role);

        return Task.CompletedTask;
    }
}
