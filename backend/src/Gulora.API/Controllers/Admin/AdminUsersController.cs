using Gulora.Application.Users.Queries.GetAdminUsers;
using Gulora.Contracts.Common;
using Gulora.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers.Admin;

/// <summary>
/// Admin kullanıcı yönetimi — listeleme ve arama.
/// Tüm uçlar Admin rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminUsersController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Tüm kullanıcıları sayfali listeler.
    /// Ad, soyad veya e-posta ile arama desteklenir; rol ve aktiflik filtresi uygulanabilir.
    /// </summary>
    /// <response code="200">Sayfalı kullanıcı özet listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetAdminUsersRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminUsersQuery(
            req.Page,
            req.PageSize,
            req.Search), ct);

        return Ok(result);
    }
}
