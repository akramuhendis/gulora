using Gulora.Application.Corporate.Commands.SubmitInquiry;
using Gulora.Contracts.Corporate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class CorporateController(IMediator mediator) : ControllerBase
{
    [HttpPost("inquiries")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> SubmitInquiry([FromBody] SubmitInquiryRequest request, CancellationToken ct)
    {
        var cmd = new SubmitInquiryCommand(
            request.Company,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Message,
            request.Occasion,
            request.Quantity,
            request.Budget);
        var id = await mediator.Send(cmd, ct);
        return Ok(id);
    }
}
