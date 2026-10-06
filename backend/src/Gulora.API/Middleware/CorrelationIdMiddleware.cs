namespace Gulora.API.Middleware;

using Microsoft.Extensions.Logging;

/// <summary>
/// Her isteğe korelasyon kimliği ekler (gulora-architecture.mdc).
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var correlationId = ctx.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = ctx.TraceIdentifier;

        ctx.Response.Headers[HeaderName] = correlationId;
        using (_loggerScope(ctx, correlationId))
        {
            await _next(ctx);
        }
    }

    private static IDisposable? _loggerScope(HttpContext ctx, string correlationId)
    {
        var loggerFactory = ctx.RequestServices.GetService<ILoggerFactory>();
        if (loggerFactory is null) return null;

        var logger = loggerFactory.CreateLogger("Correlation");
        return logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });
    }
}
