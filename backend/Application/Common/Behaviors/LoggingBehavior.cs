using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Common.Behaviors;

/// <summary>
/// Her MediatR isteği için başlama, süre ve hata (varsa) bilgisini loglar.
/// İzleme ve hata ayıklama (hangi use-case ne kadar sürdü) için faydalıdır.
/// </summary>
/// <typeparam name="TRequest">MediatR istek tipi.</typeparam>
/// <typeparam name="TResponse">Cevap tipi.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>İstek adını ve kronometreyi kaydeder, iç pipeline'ı çağırır, sonuç veya exception'ı üst katmana iletir.</summary>
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Gerçek işleyici (validasyon+transaction+handler) zincirinin tamamı burada çalışır
            var response = await next();
            sw.Stop();
            logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            // Stack'i bozma: aynı exception'ı yukarı fırlat
            sw.Stop();
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
