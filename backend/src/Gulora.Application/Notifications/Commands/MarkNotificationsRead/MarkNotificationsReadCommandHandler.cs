using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Notifications.Commands.MarkNotificationsRead;

public sealed class MarkNotificationsReadCommandHandler : IRequestHandler<MarkNotificationsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkNotificationsReadCommand cmd, CancellationToken ct)
    {
        if (cmd.NotificationIds.Count == 0)
        {
            var all = await _notificationRepository.GetUnreadByUserAsync(cmd.UserId, ct);
            foreach (var n in all) n.MarkAsRead();
        }
        else
        {
            foreach (var id in cmd.NotificationIds)
            {
                var notification = await _notificationRepository.GetByIdAsync(id, ct);
                if (notification is null || notification.UserId != cmd.UserId) continue;
                notification.MarkAsRead();
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
