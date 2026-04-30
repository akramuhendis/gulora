using Gulora.API.Extensions;
using Gulora.Application.Orders.Commands.CancelOrder;
using Gulora.Application.Orders.Commands.CreateOrder;
using Gulora.Application.Orders.Commands.RequestReturn;
using Gulora.Application.Orders.Queries.GetOrderById;
using Gulora.Application.Orders.Queries.GetOrders;
using Gulora.Contracts.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> List(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetOrdersQuery(userId), ct);
        return Ok(result);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderDto>> GetById(Guid orderId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetOrderByIdQuery(orderId, userId), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new CreateOrderCommand(
            userId,
            request.Items,
            request.Address,
            request.DeliveryType,
            request.PaymentMethod,
            request.CouponCode,
            request.Notes,
            request.ScheduledDate);
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }

    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Cancel(Guid orderId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await mediator.Send(new CancelOrderCommand(orderId, userId), ct);
        return Ok();
    }

    [HttpPost("{orderId:guid}/return")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RequestReturn(Guid orderId, [FromBody] RequestReturnRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await mediator.Send(new RequestReturnCommand(orderId, userId, request.Reason), ct);
        return Ok();
    }
}
