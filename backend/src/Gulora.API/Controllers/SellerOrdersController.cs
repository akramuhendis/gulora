using Gulora.API.Extensions;
using Gulora.Application.Orders.Commands.ShipOrder;
using Gulora.Application.Orders.Commands.UpdateOrderStatus;
using Gulora.Application.Orders.Queries.GetOrderById;
using Gulora.Application.Orders.Queries.GetSellerOrders;
using Gulora.Contracts.Common;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Satıcı paneli sipariş uçları — yalnızca bu satıcının ürünlerini içeren siparişler ve tamamlayıcı işlemler.
/// </summary>
[ApiController]
[Route("api/v1/seller/orders")]
[Produces("application/json")]
[Authorize(Roles = "Seller")]
public sealed class SellerOrdersController : ControllerBase
{
    private readonly ISender _mediator;

    public SellerOrdersController(ISender mediator) => _mediator = mediator;

    /// <summary>Satıcının ürün kalemleri bulunan siparişleri sayfalı listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] GetSellerOrdersRequest req, CancellationToken ct)
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

        var sellerId = User.GetUserId();
        var result = await _mediator.Send(new GetSellerOrdersQuery(
            sellerId,
            req.Page,
            req.PageSize,
            status,
            req.OrderNumber,
            req.DateFrom,
            req.DateTo), ct);

        return Ok(result);
    }

    /// <summary>Sipariş detayı — satıcının ürünü siparişte yer alıyorsa görüntülenebilir.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var sellerId = User.GetUserId();
        var result = await _mediator.Send(
            new GetOrderByIdQuery(id, sellerId, IsAdmin: false, SellerScopeUserId: sellerId),
            ct);
        return Ok(result);
    }

    /// <summary>
    /// Sipariş durumunu günceller. Sipariş yalnızca bu satıcıya ait ürünlerden oluşuyorsa uygulanır
    /// (birden fazla satıcı varsa 403).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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

        var sellerId = User.GetUserId();
        await _mediator.Send(
            new UpdateOrderStatusCommand(id, sellerId, newStatus, SellerOperatorId: sellerId),
            ct);
        return NoContent();
    }

    /// <summary>
    /// Siparişi kargoya verir. Sepette yalnızca bu satıcının ürünleri varsa mümkündür.
    /// </summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShipOrder(
        Guid id, [FromBody] ShipOrderRequest req, CancellationToken ct)
    {
        var sellerId = User.GetUserId();
        await _mediator.Send(new ShipOrderCommand(
            id,
            sellerId,
            req.CargoCompany,
            req.TrackingNumber,
            SellerOperatorId: sellerId), ct);
        return NoContent();
    }
}
