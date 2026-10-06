using Gulora.Contracts.Common;
using Gulora.Contracts.Notifications;
using MediatR;

namespace Gulora.Application.Notifications.Queries.GetNotifications;

/// <summary>Kullanıcının bildirimlerini sayfalı getirir.</summary>
public sealed record GetNotificationsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<NotificationResponse>>;
