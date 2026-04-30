using System.Reflection;
using FluentValidation;
using Gulora.Application.Common.Behaviors;
using Gulora.Application.Mappings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Gulora.Application;

/// <summary>
/// <see cref="Gulora.Application"/> (uygulama) katmanının, bağımlılık enjeksiyonu konteynerine
/// (MediatR, FluentValidation, AutoMapper, pipeline davranışları) nasıl ekleneceğini tanımlar.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Bu assembly içindeki tüm CQRS işleyicileri, doğrulayıcıları, AutoMapper profilleri ve
    /// MediatR pipeline katmanlarını (loglama, validasyon, komutlar için transaction) DI&apos;a kaydeder.
    /// </summary>
    /// <param name="services">ASP.NET Core hizmet koleksiyonu.</param>
    /// <returns>Konfigüre edilmiş hizmet koleksiyonu (akışa devam edilebilir).</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Tüm command/query/notification handler'ları aynı assembly'den tara
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR: IRequestHandler, INotificationHandler implementasyonlarını bul ve kaydet
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation: tüm AbstractValidator türevlerini (internal tipler dahil) bul ve kaydet
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // AutoMapper: v15+ sürümünde assembly yerine, profil içeren bir "işaret" tipi verilir
        services.AddAutoMapper(static _ => { }, typeof(OrderMappingProfile));

        // Pipeline sırası: önce loglama, sonra FluentValidation, en son (yalnız ICommand) transaction
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
