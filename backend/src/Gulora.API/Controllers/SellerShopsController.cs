using Gulora.Application.SellerCampaignPlans.Queries.ListPublicActiveCampaignPlans;
using Gulora.Application.SellerCampaignPlans.Queries.ListPublicSellerCampaignPlansBySlug;
using Gulora.Application.SellerShops.Commands.UpsertMySellerShop;
using Gulora.Application.SellerShops.Queries.GetMySellerShop;
using Gulora.Application.SellerShops.Queries.GetSellerShopBySlug;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Contracts.SellerShops;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>Satıcı mağaza vitrin bilgisi — herkese açık slug ile mağaza sayfası.</summary>
[ApiController]
[Route("api/v1/seller-shops")]
[Produces("application/json")]
public sealed class SellerShopsController : ControllerBase
{
    private readonly ISender _mediator;

    public SellerShopsController(ISender mediator) => _mediator = mediator;

    /// <summary>Bütün mağazalar üçün hazırda aktiv kampaniya planları (ana səhifə / vitrin).</summary>
    [HttpGet("active-campaigns")]
    [ProducesResponseType(typeof(IReadOnlyList<SellerCampaignPlanPublicHomeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListActiveCampaignsForHome(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListPublicActiveCampaignPlansQuery(), ct);
        return Ok(result);
    }

    /// <summary>Mağaza adresi (slug) ilə vitrin məlumatı — <c>.../seller-shops/view/{slug}</c></summary>
    [HttpGet("view/{slug}")]
    [ProducesResponseType(typeof(SellerShopPublicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSellerShopBySlugQuery(slug), ct);
        return Ok(result);
    }

    /// <summary>Mağaza üçün hazırda aktiv kampaniya planları (ictimai).</summary>
    [HttpGet("view/{slug}/campaigns")]
    [ProducesResponseType(typeof(IReadOnlyList<SellerCampaignPlanPublicResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListActiveCampaignsBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new ListPublicSellerCampaignPlansBySlugQuery(slug), ct);
        return Ok(result);
    }

    /// <summary>Oturum açmış satıcının mağaza kaydı (yoksa 404).</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(SellerShopMyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMySellerShopQuery(), ct);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>Mağaza bilgisini oluşturur veya günceller.</summary>
    [HttpPut("me")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(SellerShopMyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMine(
        [FromBody] UpsertSellerShopRequest req,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpsertMySellerShopCommand(
                req.StoreName,
                req.StoreSlug,
                req.Description,
                req.LogoUrl,
                req.BannerUrl),
            ct);
        return Ok(result);
    }
}
