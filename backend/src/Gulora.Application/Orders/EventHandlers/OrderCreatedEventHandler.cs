using Gulora.Application.Common.Interfaces;
using Gulora.Application.Common.Notifications;
using Gulora.Domain.Events;
using Gulora.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Orders.EventHandlers;

/// <summary>
/// Sipariş oluşturulduğunda tetiklenir — kullanıcıya onay e-postası gönderir.
/// Infrastructure SaveChanges'dan sonra DomainEventNotification olarak yayınlar.
/// </summary>
public sealed class OrderCreatedEventHandler
    : INotificationHandler<DomainEventNotification<OrderCreatedEvent>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<OrderCreatedEvent> notification,
        CancellationToken ct)
    {
        var evt = notification.DomainEvent;

        var user = await _userRepository.GetByIdAsync(evt.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning(
                "OrderCreatedEventHandler: UserId {UserId} bulunamadı. OrderId: {OrderId}",
                evt.UserId, evt.OrderId);
            return;
        }

        await _emailService.SendOrderConfirmationAsync(
            user.Email, user.FullName, evt.OrderNumber, evt.TotalAmount, ct);

        _logger.LogInformation(
            "Sipariş onay e-postası gönderildi. OrderNumber: {OrderNumber}",
            evt.OrderNumber);
    }
}
