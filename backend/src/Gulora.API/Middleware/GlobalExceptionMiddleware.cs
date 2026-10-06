using Gulora.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Middleware;

/// <summary>
/// Global hata yakalama middleware'i — domain istisnaları HTTP ProblemDetails'e dönüştürür.
/// Stack trace ve iç yol bilgisi kullanıcıya asla verilmez (gulora-security.mdc).
/// Yerleşim: UseHttpsRedirection'dan önce, pipeline'ın en başına eklenmelidir.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status404NotFound,
                "Bulunamadı", ex.Message);
        }
        catch (ValidationException ex)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            ctx.Response.ContentType = "application/problem+json";
            var distinctMessages = ex.Errors.Values
                .SelectMany(v => v)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToArray();
            var detail = distinctMessages.Length > 0
                ? string.Join(' ', distinctMessages)
                : ex.Message;
            await ctx.Response.WriteAsJsonAsync(new ValidationProblemDetails(
                ex.Errors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Doğrulama hatası",
                Detail = detail
            });
        }
        catch (BusinessRuleException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status400BadRequest,
                "İş kuralı ihlali", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status409Conflict,
                "Çakışma", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status401Unauthorized,
                "Kimlik doğrulama gerekli", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status403Forbidden,
                "Yetki yok", ex.Message);
        }
        catch (TooManyRequestsException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status429TooManyRequests,
                "Çok fazla istek", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "İşlenmemiş hata — Path: {Path}, TraceId: {TraceId}",
                ctx.Request.Path,
                ctx.TraceIdentifier);

            var detail = _env.IsDevelopment()
                ? ex.Message
                : "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";

            await WriteProblemAsync(ctx, StatusCodes.Status500InternalServerError,
                "Sunucu hatası", detail);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext ctx, int statusCode, string title, string detail)
    {
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        });
    }
}
