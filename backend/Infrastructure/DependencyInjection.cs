using Gulora.Application.Common.Interfaces;
using Gulora.Infrastructure.Options;
using Gulora.Infrastructure.Persistence;
using Gulora.Infrastructure.Persistence.Repositories;
using Gulora.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gulora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<ICouponRepository, EfCouponRepository>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();
        services.AddScoped<IAddressRepository, EfAddressRepository>();
        services.AddScoped<IFavoriteRepository, EfFavoriteRepository>();
        services.AddScoped<IReviewRepository, EfReviewRepository>();
        services.AddScoped<ICorporateInquiryRepository, EfCorporateInquiryRepository>();
        services.AddScoped<ISellerApplicationRepository, EfSellerApplicationRepository>();

        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<GuloraDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        return services;
    }
}
