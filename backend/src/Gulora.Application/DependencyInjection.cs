using System.Reflection;
using FluentValidation;
using Gulora.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Gulora.Application;

/// <summary>
/// Application katmanı DI kaydı.
/// Kanonik pipeline sırası: LoggingBehavior → ValidationBehavior → Handler.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline sırası birebir bu listenin sırasıyla uyumludur (gulora-cqrs-patterns.mdc).
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
