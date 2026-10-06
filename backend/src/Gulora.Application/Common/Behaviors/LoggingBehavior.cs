using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Common.Behaviors;

/// <summary>
/// Pipeline davranışı — istek giriş/çıkış zamanlaması ve hata seviyeli loglama.
/// PII ve token hiçbir zaman loglanmaz (gulora-security.mdc).
/// Kanonik sıra: LoggingBehavior → ValidationBehavior → Handler.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            sw.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs} ms",
                requestName,
                sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMs} ms",
                requestName,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
