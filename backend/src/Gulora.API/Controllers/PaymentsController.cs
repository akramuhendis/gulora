using Gulora.Application.Payments.Commands.UpdatePaymentStatus;
using Gulora.Contracts.Payments;
using Gulora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Ödeme webhook uçları — ödeme sağlayıcısından gelen durum güncellemelerini işler.
/// İmza doğrulaması altyapı/middleware düzeyinde uygulanmalıdır (gulora-security.mdc).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender _mediator;

    public PaymentsController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Ödeme sağlayıcısı webhook'u — ödeme durumunu günceller.
    /// İdempotency key tekrarlı işlemi önler.
    /// </summary>
    /// <param name="idempotencyKey">Ödeme sağlayıcısından gelen benzersiz işlem anahtarı.</param>
    /// <response code="200">Durum güncellendi.</response>
    /// <response code="400">Geçersiz durum değeri veya doğrulama hatası.</response>
    [HttpPost("webhook/{idempotencyKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PaymentWebhook(
        string idempotencyKey,
        [FromBody] UpdatePaymentStatusRequest req,
        CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentStatus>(req.NewStatus, ignoreCase: true, out var paymentStatus))
            return BadRequest(new ProblemDetails
            {
                Title = "Geçersiz ödeme durumu",
                Detail = $"Geçerli değerler: {string.Join(", ", Enum.GetNames<PaymentStatus>())}",
                Status = StatusCodes.Status400BadRequest
            });

        await _mediator.Send(new UpdatePaymentStatusCommand(
            idempotencyKey,
            paymentStatus,
            req.ProviderTransactionId,
            req.FailureReason), ct);

        return Ok();
    }
}
