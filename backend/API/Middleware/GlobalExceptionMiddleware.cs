using System.Net;
using System.Text.Json;
using FluentValidation;
using Gulora.Domain.Exceptions;

namespace Gulora.API.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            var body = new
            {
                title = "Doğrulama xətası",
                status = 400,
                errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
        catch (NotFoundException ex)
        {
            await WriteJsonProblemAsync(context, ex.Message, StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedException ex)
        {
            await WriteJsonProblemAsync(context, ex.Message, StatusCodes.Status401Unauthorized);
        }
        catch (DomainException ex)
        {
            await WriteJsonProblemAsync(context, ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteJsonProblemAsync(context, "Daxili server xətası.", StatusCodes.Status500InternalServerError);
        }
    }

    private static Task WriteJsonProblemAsync(HttpContext context, string title, int status)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { title, status }));
    }
}
