using Gulora.Application.Common.Extensions;
using Gulora.Application.Common.Interfaces;
using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using Gulora.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Orders.EventHandlers;

/// <summary>
/// Sipariş durumu değiştiğinde tetiklenir — kullanıcıya bildirim e-postası gönderir.
/// </summary>
public sealed class OrderStatusChangedEventHandler
    : INotificationHandler<DomainEventNotification<OrderStatusChangedEvent>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<OrderStatusChangedEvent> notification,
        CancellationToken ct)
    {
        var evt = notification.DomainEvent;

        var user = await _userRepository.GetByIdAsync(evt.UserId, ct);
        if (user is null) return;

        await _emailService.SendOrderStatusChangedAsync(
            user.Email,
            user.FullName,
            evt.OrderNumber,
            evt.NewStatus.ToDisplayString(),
            ct);

        _logger.LogInformation(
            "Sipariş durum bildirimi gönderildi. OrderNumber: {OrderNumber}, NewStatus: {Status}",
            evt.OrderNumber, evt.NewStatus);
    }
}
