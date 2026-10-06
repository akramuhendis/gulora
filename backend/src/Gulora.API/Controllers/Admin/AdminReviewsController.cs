using Gulora.Application.Reviews.Commands.ModerateReview;
using Gulora.Application.Reviews.Queries.GetAdminReviews;
using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers.Admin;

/// <summary>
/// Admin yorum moderasyon paneli — listeleme, onaylama ve reddetme.
/// Tüm uçlar Admin rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/admin/reviews")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class AdminReviewsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminReviewsController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Yorumları durum ve ürüne göre filtreli sayfali listeler.
    /// Varsayılan olarak bekleyen (Pending) yorumlar gelir.
    /// </summary>
    /// <response code="200">Sayfalı yorum listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(
        [FromQuery] GetAdminReviewsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminReviewsQuery(
            req.Page,
            req.PageSize,
            req.Status,
            req.ProductId), ct);

        return Ok(result);
    }

    /// <summary>Yorumu onaylar ve yayına alır.</summary>
    /// <response code="204">Yorum onaylandı.</response>
    /// <response code="404">Yorum bulunamadı.</response>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveReview(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ModerateReviewCommand(id, Approve: true), ct);
        return NoContent();
    }

    /// <summary>Yorumu reddeder. Red gerekçesi zorunludur.</summary>
    /// <response code="204">Yorum reddedildi.</response>
    /// <response code="400">Red gerekçesi eksik.</response>
    /// <response code="404">Yorum bulunamadı.</response>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectReview(
        Guid id, [FromBody] RejectReviewRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ModerateReviewCommand(
            id, Approve: false, RejectionReason: req.Reason), ct);
        return NoContent();
    }
}
