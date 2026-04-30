using Gulora.Application.Coupons.Queries.ValidateCoupon;
using Gulora.Contracts.Coupons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CouponsController(IMediator mediator) : ControllerBase
{
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateCouponResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateCouponResponse>> Validate([FromBody] ValidateCouponRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new ValidateCouponQuery(request.Code, request.OrderAmount), ct);
        return Ok(result);
    }
}
