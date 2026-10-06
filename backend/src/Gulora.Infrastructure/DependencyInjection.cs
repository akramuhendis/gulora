using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Infrastructure.Identity;
using Gulora.Infrastructure.Persistence;
using Gulora.Infrastructure.Persistence.Interceptors;
using Gulora.Infrastructure.Persistence.Repositories;
using Gulora.Infrastructure.Services.Auth;
using Gulora.Infrastructure.Services.Domain;
using Gulora.Infrastructure.Services.Email;
using Gulora.Infrastructure.Services.Sms;
using Gulora.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace Gulora.Infrastructure;

/// <summary>
/// Infrastructure katmanı DI kaydı.
/// API Program.cs içinde Application kaydından sonra çağrılır:
/// <c>services.AddApplication(); services.AddInfrastructure(configuration, environment);</c>
/// gulora-infrastructure-contracts.mdc: AddInfrastructure extension metodu.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddPersistence(configuration)
            .AddAuthServices(configuration)
            .AddApplicationServices(configuration, environment)
            .AddDomainServices()
            .AddHttpContextAccessor();

        return services;
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DomainEventSaveChangesInterceptor>();

        services.AddDbContext<GuloraDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<DomainEventSaveChangesInterceptor>();

            options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsql =>
                    {
                        npgsql.MigrationsAssembly(typeof(GuloraDbContext).Assembly.FullName);
                        npgsql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    })
                .AddInterceptors(interceptor);
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IDiscountCouponRepository, DiscountCouponRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IPhoneOtpRepository, PhoneOtpRepository>();
        services.AddScoped<ISellerApplicationRepository, SellerApplicationRepository>();
        services.AddScoped<ISellerShopRepository, SellerShopRepository>();
        services.AddScoped<ISellerCampaignPlanRepository, SellerCampaignPlanRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // ── Auth (JWT + BCrypt) ───────────────────────────────────────────────────

    private static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "Jwt:SigningKey yapılandırılmamış. dotnet user-secrets veya ortam değişkeni kullanın.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Etibarsız / vaxtı keçmiş Bearer token — ictimai GET uçlarına (məs. kateqoriyalar) 401 verməsin;
                // [Authorize] olan sorğularda istifadəçi anonim qalır və 401 düzgün qaytarılır.
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }

    // ── Uygulama servisleri (e-posta, depolama) ───────────────────────────────

    private static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (IsEmailSendingEnabled(configuration, environment))
            services.AddScoped<IEmailService, MailKitEmailService>();
        else
            services.AddScoped<IEmailService, NoOpEmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ISmsService, SmsService>();

        return services;
    }

    /// <summary>
    /// <c>Email:Enabled</c> açıkça verilmişse onu kullanır; yoksa Development'ta kapalı,
    /// diğer ortamlarda açık (SMTP) varsayılır — yerel çalıştırmada yapılandırma eksikse MailKit çağrılmaz.
    /// </summary>
    private static bool IsEmailSendingEnabled(IConfiguration configuration, IHostEnvironment environment)
    {
        var raw = configuration["Email:Enabled"];
        if (!string.IsNullOrWhiteSpace(raw) && bool.TryParse(raw, out var parsed))
            return parsed;

        return !environment.IsDevelopment();
    }

    // ── Domain servisleri ─────────────────────────────────────────────────────

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderNumberService, OrderNumberService>();
        services.AddScoped<IDeliveryFeeCalculator, DeliveryFeeCalculatorService>();
        services.AddScoped<IDiscountCalculator, DiscountCalculatorService>();

        return services;
    }
}
