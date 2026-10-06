using Gulora.API.Extensions;
using Gulora.Application.Auth.Commands.ForgotPassword;
using Gulora.Application.Auth.Commands.Login;
using Gulora.Application.Auth.Commands.Logout;
using Gulora.Application.Auth.Commands.RefreshToken;
using Gulora.Application.Auth.Commands.Register;
using Gulora.Application.Auth.Commands.ResetPassword;
using Gulora.Application.Auth.Commands.SendOtp;
using Gulora.Application.Auth.Commands.VerifyEmail;
using Gulora.Application.Auth.Commands.VerifyOtp;
using Gulora.Application.Auth.Queries.GetCurrentUser;
using Gulora.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gulora.API.Controllers;

/// <summary>
/// Kimlik doğrulama uçları — JWT çifti, parola sıfırlama ve token yenileme.
/// Login / Register / ForgotPassword uçlarına FixedWindow rate-limit uygulanır.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    /// <summary>Kullanıcı girişi — JWT erişim ve yenileme token çifti döner.</summary>
    /// <response code="200">Giriş başarılı; token çifti ve kullanıcı özeti.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="401">E-posta veya parola hatalı.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(req.Email, req.Password), ct);
        return Ok(result);
    }

    /// <summary>Yeni kullanıcı kaydı — başarıda JWT çifti döner.</summary>
    /// <response code="201">Kayıt başarılı.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="409">E-posta zaten kayıtlı.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(
            req.FirstName, req.LastName, req.Email, req.Password, req.PhoneNumber), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Oturumu kapatır — aktif refresh token'ı iptal eder.</summary>
    /// <response code="204">Çıkış başarılı.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(req.RefreshToken), ct);
        return NoContent();
    }

    /// <summary>Refresh token ile yeni JWT çifti alır.</summary>
    /// <response code="200">Token yenileme başarılı.</response>
    /// <response code="401">Refresh token geçersiz veya süresi dolmuş.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(req.RefreshToken), ct);
        return Ok(result);
    }

    /// <summary>Parola sıfırlama e-postası gönderir.</summary>
    /// <response code="200">
    /// İstek alındı. E-posta kayıtlıysa sıfırlama bağlantısı gönderilir
    /// (kullanıcı varlığı sızdırılmaz).
    /// </response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ForgotPasswordCommand(req.Email), ct);
        return Ok();
    }

    /// <summary>Token ile yeni parola belirler.</summary>
    /// <response code="204">Parola başarıyla sıfırlandı.</response>
    /// <response code="400">Token geçersiz, süresi dolmuş veya doğrulama hatası.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ResetPasswordCommand(
            req.Token, req.NewPassword, req.ConfirmNewPassword), ct);
        return NoContent();
    }

    /// <summary>Kayıt sonrası e-posta doğrulama jetonunu doğrular.</summary>
    /// <response code="204">E-posta doğrulandı.</response>
    /// <response code="400">Jeton geçersiz veya süresi dolmuş.</response>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest req, CancellationToken ct)
    {
        await _mediator.Send(new VerifyEmailCommand(req.Token), ct);
        return NoContent();
    }

    /// <summary>Telefon numarasına SMS OTP gönderir.</summary>
    /// <response code="200">Kod gönderildi.</response>
    /// <response code="429">Çok sık istek.</response>
    [HttpPost("otp/send")]
    [AllowAnonymous]
    [EnableRateLimiting("OtpPolicy")]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendOtpCommand(req.PhoneNumber), ct);
        return Ok(result);
    }

    /// <summary>Gönderilen OTP kodunu doğrular; oturum varsa telefon hesaba bağlanır.</summary>
    [HttpPost("otp/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req, CancellationToken ct)
    {
        var linkUserId = User.TryGetUserId();
        var result = await _mediator.Send(
            new VerifyOtpCommand(req.PhoneNumber, req.Code, linkUserId), ct);
        return Ok(result);
    }

    /// <summary>Token sahibinin kimlik ve profil özetini döner.</summary>
    /// <response code="200">Mevcut kullanıcı özeti.</response>
    /// <response code="401">Token eksik veya geçersiz.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetCurrentUserQuery(userId), ct);
        return Ok(result);
    }
}
