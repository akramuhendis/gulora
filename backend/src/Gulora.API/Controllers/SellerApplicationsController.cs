using Gulora.Application.SellerApplications.Commands.SubmitSellerApplication;
using Gulora.Contracts.SellerApplications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gulora.API.Controllers;

/// <summary>Satıcı başvuru formu — kimlik doğrulama zorunlu değildir.</summary>
[ApiController]
[Route("api/v1/seller-applications")]
[Produces("application/json")]
public sealed class SellerApplicationsController : ControllerBase
{
    private readonly ISender _mediator;

    public SellerApplicationsController(ISender mediator) => _mediator = mediator;

    /// <summary>Yeni satıcı başvurusu oluşturur.</summary>
    /// <response code="201">Başvuru kaydedildi.</response>
    /// <response code="409">Aynı vergi numarasıyla bekleyen başvuru var.</response>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("PublicFormPolicy")]
    [ProducesResponseType(typeof(SellerApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit([FromBody] SellerApplicationRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitSellerApplicationCommand(
            req.FullName,
            req.Company,
            req.TaxNumber,
            req.Email,
            req.Phone,
            req.City,
            req.Category,
            req.Message), ct);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}
