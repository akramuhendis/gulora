using Gulora.Application.Orders.Commands.CompleteOrderReturn;
using Gulora.Application.Orders.Commands.ConfirmOrder;
using Gulora.Application.Orders.Commands.DeliverOrder;
using Gulora.Application.Orders.Commands.ShipOrder;
using Gulora.Application.Orders.Commands.StartPreparingOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public sealed class AdminOrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost("{orderId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Confirm(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new ConfirmOrderCommand(orderId), ct);
        return Ok();
    }

    [HttpPost("{orderId:guid}/start-preparing")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> StartPreparing(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new StartPreparingOrderCommand(orderId), ct);
        return Ok();
    }

    [HttpPost("{orderId:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Ship(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new ShipOrderCommand(orderId), ct);
        return Ok();
    }

    [HttpPost("{orderId:guid}/deliver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Deliver(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new DeliverOrderCommand(orderId), ct);
        return Ok();
    }

    [HttpPost("{orderId:guid}/complete-return")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CompleteReturn(Guid orderId, CancellationToken ct)
    {
        await mediator.Send(new CompleteOrderReturnCommand(orderId), ct);
        return Ok();
    }
}
