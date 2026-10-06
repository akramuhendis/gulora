using Gulora.Application.SellerCampaignPlans.Commands.CreateSellerCampaignPlan;
using Gulora.Application.SellerCampaignPlans.Commands.DeleteSellerCampaignPlan;
using Gulora.Application.SellerCampaignPlans.Commands.UpdateSellerCampaignPlan;
using Gulora.Application.SellerCampaignPlans.Queries.ListSellerCampaignPlans;
using Gulora.Contracts.SellerCampaigns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>Satıcı kampaniya planları — CRUD (vitrin qiymət motoru ayrıca).</summary>
[ApiController]
[Route("api/v1/seller/campaigns")]
[Produces("application/json")]
[Authorize(Roles = "Seller")]
public sealed class SellerCampaignPlansController : ControllerBase
{
    private readonly ISender _mediator;

    public SellerCampaignPlansController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SellerCampaignPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListSellerCampaignPlansQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SellerCampaignPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSellerCampaignPlanRequest body, CancellationToken ct)
    {
        var cmd = new CreateSellerCampaignPlanCommand(
            body.Name,
            body.Kind,
            body.Percent,
            body.StartsAt,
            body.EndsAt,
            body.MinCartAmount,
            body.Scope,
            body.Notes);

        var created = await _mediator.Send(cmd, ct);
        return Created($"/api/v1/seller/campaigns/{created.Id}", created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SellerCampaignPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSellerCampaignPlanRequest body, CancellationToken ct)
    {
        var cmd = new UpdateSellerCampaignPlanCommand(
            id,
            body.Name,
            body.Kind,
            body.Percent,
            body.StartsAt,
            body.EndsAt,
            body.MinCartAmount,
            body.Scope,
            body.Notes);

        var updated = await _mediator.Send(cmd, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSellerCampaignPlanCommand(id), ct);
        return NoContent();
    }
}
