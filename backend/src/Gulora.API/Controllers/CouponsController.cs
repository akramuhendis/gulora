using Gulora.API.Extensions;
using Gulora.Application.DiscountCoupons.Commands.CreateCoupon;
using Gulora.Application.DiscountCoupons.Commands.DeleteCoupon;
using Gulora.Application.DiscountCoupons.Queries.ValidateCoupon;
using Gulora.Contracts.DiscountCoupons;
using Gulora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// İndirim kuponları — doğrulama (müşteri) ve yönetim (Admin).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class CouponsController : ControllerBase
{
    private readonly ISender _mediator;

    public CouponsController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Ödeme öncesinde kuponu doğrular ve indirim tutarını önizler.
    /// Bu uç kuponu kullanmaz; sipariş oluşturmada gerçek uygulama yapılır.
    /// </summary>
    /// <response code="200">Doğrulama sonucu ve hesaplanan indirim tutarı.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ValidateCouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateCoupon(
        [FromQuery] ValidateCouponRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(
            new ValidateCouponQuery(req.Code, userId, req.OrderSubTotal), ct);
        return Ok(result);
    }

    /// <summary>Yeni indirim kuponu oluşturur (Admin).</summary>
    /// <response code="201">Oluşturulan kupon ID'si.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCoupon(
        [FromBody] CreateCouponRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<DiscountType>(req.Type, ignoreCase: true, out var discountType))
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz indirim türü",
                Detail = $"Geçerli değerler: {string.Join(", ", Enum.GetNames<DiscountType>())}",
                Status = StatusCodes.Status400BadRequest
            });

        var id = await _mediator.Send(new CreateCouponCommand(
            req.Code,
            discountType,
            req.Value,
            req.Description,
            req.MinOrderAmount,
            req.MaxDiscountAmount,
            req.UsageLimit,
            req.StartsAt,
            req.ExpiresAt,
            req.RestrictedUserId), ct);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    /// <summary>Kuponu devre dışı bırakır (Admin).</summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Kupon bulunamadı.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoupon(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCouponCommand(id), ct);
        return NoContent();
    }
}
