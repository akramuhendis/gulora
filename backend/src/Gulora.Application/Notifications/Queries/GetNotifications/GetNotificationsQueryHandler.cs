using Gulora.Contracts.Common;
using Gulora.Contracts.Notifications;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
        => _notificationRepository = notificationRepository;

    public async Task<PagedResult<NotificationResponse>> Handle(
        GetNotificationsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 50);
        var page = Math.Max(query.Page, 1);

        var notifications = await _notificationRepository.GetByUserPagedAsync(
            query.UserId, page, pageSize, ct);

        var totalCount = await _notificationRepository.GetTotalCountAsync(query.UserId, ct);

        var items = notifications.Select(n => new NotificationResponse(
            n.Id,
            n.Type.ToString(),
            n.Title,
            n.Body,
            n.IsRead,
            n.ReadAt,
            n.RelatedEntityId,
            n.CreatedAt)).ToList();

        return new PagedResult<NotificationResponse>(items, totalCount, page, pageSize);
    }
}
