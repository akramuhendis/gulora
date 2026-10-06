using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Gulora.API.Configuration;
using Gulora.API.Middleware;
using Gulora.API.Services;
using Gulora.Application;
using Gulora.Application.Common.Configuration;
using Gulora.Infrastructure;
using Gulora.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── CORS (Dev: Vite | Prod: gulora.az) — gulora-api-controllers.mdc ───────────
builder.Services.AddCors(o => o.AddPolicy("FrontendPolicy", p =>
    p.WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:5174",
            "http://127.0.0.1:5174",
            "http://localhost:8081",
            "http://127.0.0.1:8081",
            "http://localhost:19006",
            "http://127.0.0.1:19006",
            "https://gulora.az")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// ── Application + Infrastructure DI (sıra önemli) ───────────────────────────
builder.Services.Configure<ReviewPublishOptions>(
    builder.Configuration.GetSection(ReviewPublishOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// ── Controllers + JSON camelCase serileştirme ────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

// ── Rate Limiting — Login / Register / ForgotPassword ────────────────────────
// gulora-security.mdc: Auth uçları için FixedWindow
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("OtpPolicy", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("PublicFormPolicy", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Swagger / OpenAPI (yalnızca Development) — gulora-api-controllers.mdc ────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Gülora API", Version = "v1" });

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Bearer {token}"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Sağlık kontrolü — gulora-api-controllers.mdc ─────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<GuloraDbContext>("db", tags: ["ready"]);

// ── ProblemDetails ────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();

builder.Services.Configure<DevSellerSeedOptions>(
    builder.Configuration.GetSection(DevSellerSeedOptions.SectionName));
builder.Services.Configure<DevCatalogSeedOptions>(
    builder.Configuration.GetSection(DevCatalogSeedOptions.SectionName));
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevCatalogSeedHostedService>();
    builder.Services.AddHostedService<DevSellerSeedHostedService>();
    builder.Services.AddHostedService<DevApprovePendingReviewsHostedService>();
}

var app = builder.Build();

// ── Development: gözlənilməyən 500-ləri (cədvəl yoxdur) qarşısını almaq üçün migrasiya ──
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<GuloraDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware boru hattı (sıra kritik) ──────────────────────────────────────

// GlobalException en önce — tüm pipeline hatalarını yakalar
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gülora API v1"));
}

// Yerel Vite proxy yalnız HTTP (məs. :5284) istifadə edir; HTTPS yönləndirməsi brauzerə
// 7253-ə keçirir və fetch CORS-u pozur. Prod-da yönləndirmə aktiv qalır.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseMiddleware<PaymentWebhookAuthMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Sağlık ve hazırlık uçları — gulora-api-controllers.mdc
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

await app.RunAsync();

// Integration testler için erişilebilir
public partial class Program { }
