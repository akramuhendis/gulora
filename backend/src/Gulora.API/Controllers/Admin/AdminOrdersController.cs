using Gulora.API.Extensions;
using Gulora.Application.Orders.Commands.ShipOrder;
using Gulora.Application.Orders.Commands.UpdateOrderStatus;
using Gulora.Application.Orders.Queries.GetAdminOrders;
using Gulora.Application.Orders.Queries.GetOrderById;
using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers.Admin;

/// <summary>
/// Admin sipariş yönetimi — listeleme, durum güncelleme ve kargolama.
/// Tüm uçlar Admin rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/admin/orders")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminOrdersController(ISender mediator) => _mediator = mediator;

    /// <summary>Tüm siparişleri filtreli sayfali listeler.</summary>
    /// <response code="200">Sayfalı sipariş listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] GetAdminOrdersRequest req, CancellationToken ct)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            if (!Enum.TryParse<OrderStatus>(req.Status, ignoreCase: true, out var parsed))
                return BadRequest(new ProblemDetails
                {
                    Title = "Geçersiz sipariş durumu",
                    Detail = $"Geçerli değerler: {string.Join(", ", Enum.GetNames<OrderStatus>())}",
                    Status = StatusCodes.Status400BadRequest
                });
            status = parsed;
        }

        var result = await _mediator.Send(new GetAdminOrdersQuery(
            req.Page,
            req.PageSize,
            status,
            req.UserId,
            req.OrderNumber,
            req.DateFrom,
            req.DateTo), ct);

        return Ok(result);
    }

    /// <summary>Herhangi bir siparişin detayını getirir (admin erişimi).</summary>
    /// <response code="200">Sipariş detayı.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var adminId = User.GetUserId();
        var result = await _mediator.Send(
            new GetOrderByIdQuery(id, adminId, IsAdmin: true), ct);
        return Ok(result);
    }

    /// <summary>Sipariş durumunu günceller (Confirm, Processing, Delivered vb.).</summary>
    /// <response code="204">Durum güncellendi.</response>
    /// <response code="400">Geçersiz durum geçişi.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateOrderStatusRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(req.NewStatus, ignoreCase: true, out var newStatus))
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz sipariş durumu",
                Detail = $"Geçerli değerler: {string.Join(", ", Enum.GetNames<OrderStatus>())}",
                Status = StatusCodes.Status400BadRequest
            });

        var adminId = User.GetUserId();
        await _mediator.Send(new UpdateOrderStatusCommand(id, adminId, newStatus), ct);
        return NoContent();
    }

    /// <summary>Siparişi kargoya verir (Processing → Shipped).</summary>
    /// <response code="204">Kargolama başarılı.</response>
    /// <response code="400">Geçersiz durum geçişi.</response>
    /// <response code="404">Sipariş bulunamadı.</response>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShipOrder(
        Guid id, [FromBody] ShipOrderRequest req, CancellationToken ct)
    {
        var adminId = User.GetUserId();
        await _mediator.Send(new ShipOrderCommand(
            id, adminId, req.CargoCompany, req.TrackingNumber), ct);
        return NoContent();
    }
}
