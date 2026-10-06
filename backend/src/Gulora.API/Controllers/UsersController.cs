using Gulora.API.Extensions;
using Gulora.Application.Users.Commands.AddAddress;
using Gulora.Application.Users.Commands.ChangePassword;
using Gulora.Application.Users.Commands.DeleteAddress;
using Gulora.Application.Users.Commands.UpdateAddress;
using Gulora.Application.Users.Commands.UpdateProfile;
using Gulora.Application.Users.Queries.GetMyProfile;
using Gulora.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Oturum açmış kullanıcının profili ve teslimat adresleri.
/// Tüm uçlar JWT ile kimlik doğrulaması gerektirir.
/// Admin kullanıcı listeleme için bkz. <c>api/v1/admin/users</c>.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator) => _mediator = mediator;

    // ── Profil ──────────────────────────────────────────────────────────────────

    /// <summary>Oturum açmış kullanıcının tam profilini (adresler dahil) getirir.</summary>
    /// <response code="200">Profil ve adres listesi.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetMyProfileQuery(userId), ct);
        return Ok(result);
    }

    /// <summary>Kullanıcı adı, soyadı ve telefon numarasını günceller.</summary>
    /// <response code="200">Güncel profil.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new UpdateProfileCommand(
            userId, req.FirstName, req.LastName, req.PhoneNumber), ct);

        var updated = await _mediator.Send(new GetMyProfileQuery(userId), ct);
        return Ok(updated);
    }

    /// <summary>Mevcut parolayı doğrulayarak yeni parola belirler.</summary>
    /// <response code="204">Parola değiştirildi.</response>
    /// <response code="400">Doğrulama hatası veya mevcut parola yanlış.</response>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new ChangePasswordCommand(
            userId, req.CurrentPassword, req.NewPassword), ct);
        return NoContent();
    }

    // ── Adresler ────────────────────────────────────────────────────────────────

    /// <summary>Kullanıcıya yeni teslimat adresi ekler.</summary>
    /// <response code="201">Oluşturulan adresin ID'si döner.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpPost("me/addresses")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAddress(
        [FromBody] AddAddressRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var id = await _mediator.Send(new AddAddressCommand(
            userId,
            req.Title,
            req.RecipientName,
            req.PhoneNumber,
            req.City,
            req.District,
            req.FullLine,
            req.PostalCode,
            req.CountryCode,
            req.IsDefault), ct);

        return CreatedAtAction(nameof(GetMyProfile), new { }, id);
    }

    /// <summary>Belirtilen teslimat adresini günceller.</summary>
    /// <response code="200">Güncel profil.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="404">Adres bulunamadı.</response>
    [HttpPut("me/addresses/{id:guid}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(
        Guid id, [FromBody] UpdateAddressRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new UpdateAddressCommand(
            userId,
            id,
            req.Title,
            req.RecipientName,
            req.PhoneNumber,
            req.City,
            req.District,
            req.FullLine,
            req.PostalCode), ct);

        var updated = await _mediator.Send(new GetMyProfileQuery(userId), ct);
        return Ok(updated);
    }

    /// <summary>Belirtilen teslimat adresini siler.</summary>
    /// <response code="204">Adres silindi.</response>
    /// <response code="404">Adres bulunamadı.</response>
    [HttpDelete("me/addresses/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new DeleteAddressCommand(userId, id), ct);
        return NoContent();
    }
}
