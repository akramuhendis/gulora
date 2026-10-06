using Gulora.Application.DiscountCoupons.Commands.UpdateCoupon;
using Gulora.Application.DiscountCoupons.Queries.GetAdminCoupons;
using Gulora.Contracts.Common;
using Gulora.Contracts.DiscountCoupons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers.Admin;

/// <summary>
/// Admin kupon meta güncelleme — kod/tür/tutar değişmez.
/// </summary>
[ApiController]
[Route("api/v1/admin/coupons")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class AdminCouponsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminCouponsController(ISender mediator) => _mediator = mediator;

    /// <summary>Tüm kuponları sayfalı listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CouponResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCoupons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminCouponsQuery(page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Kuponun açıklama, tarih ve aktiflik alanlarını günceller.</summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Kupon bulunamadı.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCoupon(
        Guid id, [FromBody] UpdateCouponRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateCouponCommand(
            id,
            req.Description,
            req.MinOrderAmount,
            req.MaxDiscountAmount,
            req.UsageLimit,
            req.StartsAt,
            req.ExpiresAt,
            req.IsActive), ct);

        return NoContent();
    }
}
