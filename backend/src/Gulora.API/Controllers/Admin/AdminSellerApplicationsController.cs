using Gulora.Application.SellerApplications.Commands.ApproveSellerApplication;
using Gulora.Application.SellerApplications.Commands.RejectSellerApplication;
using Gulora.Application.SellerApplications.Queries.GetAdminSellerApplications;
using Gulora.Contracts.Common;
using Gulora.Contracts.SellerApplications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers.Admin;

/// <summary>Admin satıcı başvuru moderasyonu.</summary>
[ApiController]
[Route("api/v1/admin/seller-applications")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class AdminSellerApplicationsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminSellerApplicationsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SellerApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAdminSellerApplicationsQuery(page, pageSize, status), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ApproveSellerApplicationCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectSellerApplicationRequest req,
        CancellationToken ct)
    {
        await _mediator.Send(new RejectSellerApplicationCommand(id, req.Reason), ct);
        return NoContent();
    }
}
