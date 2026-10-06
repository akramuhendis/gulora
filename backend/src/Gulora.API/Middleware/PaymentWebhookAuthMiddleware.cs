namespace Gulora.API.Middleware;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Ödeme webhook uçlarında paylaşılan sır doğrulaması (gulora-security.mdc).
/// Header: <c>X-Gulora-Webhook-Secret</c>
/// </summary>
public sealed class PaymentWebhookAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public PaymentWebhookAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!IsPaymentWebhookPath(ctx.Request.Path))
        {
            await _next(ctx);
            return;
        }

        var expected = _configuration["Payment:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await ctx.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Webhook yapılandırılmamış",
                Detail = "Payment:WebhookSecret ortam değişkeni tanımlanmalıdır.",
                Status = StatusCodes.Status503ServiceUnavailable
            });
            return;
        }

        var provided = ctx.Request.Headers["X-Gulora-Webhook-Secret"].FirstOrDefault();
        if (!string.Equals(expected, provided, StringComparison.Ordinal))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Yetkisiz webhook",
                Detail = "Geçersiz veya eksik webhook sırrı.",
                Status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        await _next(ctx);
    }

    private static bool IsPaymentWebhookPath(PathString path)
        => path.StartsWithSegments("/api/v1/payments/webhook", StringComparison.OrdinalIgnoreCase);
}
