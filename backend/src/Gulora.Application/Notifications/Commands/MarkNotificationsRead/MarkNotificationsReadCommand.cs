using MediatR;

namespace Gulora.Application.Notifications.Commands.MarkNotificationsRead;

/// <summary>
/// Belirtilen bildirimleri okundu olarak işaretler.
/// NotificationIds boşsa kullanıcının tüm bildirimleri okundu yapılır.
/// </summary>
public sealed record MarkNotificationsReadCommand(
    Guid UserId,
    IReadOnlyList<Guid> NotificationIds
) : IRequest;
