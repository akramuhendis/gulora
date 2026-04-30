using Gulora.Application.Sellers.Commands.ApplyAsSeller;
using Gulora.Contracts.Sellers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SellersController(IMediator mediator) : ControllerBase
{
    [HttpPost("apply")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> Apply([FromBody] ApplyRequest request, CancellationToken ct)
    {
        var cmd = new ApplyAsSellerCommand(
            request.FullName,
            request.Company,
            request.TaxNumber,
            request.Email,
            request.Phone,
            request.City,
            request.Category,
            request.Message);
        var id = await mediator.Send(cmd, ct);
        return Ok(id);
    }
}
