using Gulora.Application.Corporate.Commands.SubmitInquiry;
using Gulora.Contracts.Corporate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gulora.API.Controllers;

/// <summary>
/// Kurumsal / B2B talep formu — kimlik doğrulama zorunlu değildir.
/// Rate-limit ile tekrarlı gönderimler sınırlandırılır.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class CorporateController : ControllerBase
{
    private readonly ISender _mediator;

    public CorporateController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Kurumsal talep formu gönderir.
    /// Başarı durumunda admin adresine e-posta iletilir.
    /// </summary>
    /// <response code="200">Form alındı; işleme alındı.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpPost("inquiries")]
    [EnableRateLimiting("PublicFormPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitInquiry(
        [FromBody] SubmitCorporateInquiryRequest req, CancellationToken ct)
    {
        await _mediator.Send(new SubmitCorporateInquiryCommand(
            req.Company,
            req.ContactName,
            req.Email,
            req.Phone,
            req.Occasion,
            req.Quantity,
            req.Budget,
            req.Message), ct);

        return Ok();
    }
}
