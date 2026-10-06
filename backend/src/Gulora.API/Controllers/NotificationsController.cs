using Gulora.API.Extensions;
using Gulora.Application.Notifications.Commands.MarkNotificationsRead;
using Gulora.Application.Notifications.Queries.GetNotifications;
using Gulora.Contracts.Common;
using Gulora.Contracts.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Kullanıcı bildirimleri — listeleme ve toplu okundu işareti.
/// Tüm uçlar kimlik doğrulaması gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationsController(ISender mediator) => _mediator = mediator;

    /// <summary>Kullanıcının bildirimlerini sayfali getirir.</summary>
    /// <response code="200">Sayfalı bildirim listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] GetNotificationsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(
            new GetNotificationsQuery(userId, req.Page, req.PageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen bildirimleri okundu işaretler.
    /// Body boş ise tüm okunmamış bildirimler toplu işaretlenir.
    /// </summary>
    /// <response code="204">İşaretleme başarılı.</response>
    [HttpPatch("read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkNotificationsReadRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new MarkNotificationsReadCommand(
            userId, req.NotificationIds), ct);
        return NoContent();
    }
}
