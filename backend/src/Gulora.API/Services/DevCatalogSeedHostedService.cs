using Gulora.API.Configuration;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Gulora.API.Services;

/// <summary>
/// Development ortamında heç bir kateqoriya yoxdursa aktiv kök (və bir alt) kateqoriya yaradır —
/// satıcı məhsul formu və vitrin üçün minimal kataloq təmin edir.
/// </summary>
public sealed class DevCatalogSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevCatalogSeedHostedService> _logger;
    private readonly DevCatalogSeedOptions _options;

    public DevCatalogSeedHostedService(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<DevCatalogSeedHostedService> logger,
        IOptions<DevCatalogSeedOptions> options)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() || !_options.Enabled)
            return;

        using var scope = _scopeFactory.CreateScope();
        var categories = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var existing = await categories.CountAsync(null, cancellationToken);
        if (existing > 0)
        {
            _logger.LogInformation(
                "DevCatalogSeed: bazada {Count} kateqoriya var — toxum atlanır.",
                existing);
            return;
        }

        var roots = new[]
        {
            Category.Create(
                name: "Gül və buketlər",
                slug: "gul-ve-buketler",
                description: "Development üçün nümunə kateqoriya.",
                displayOrder: 1),
            Category.Create(
                name: "Hədiyyələr",
                slug: "hediyyeler",
                description: "Development üçün nümunə kateqoriya.",
                displayOrder: 2),
            Category.Create(
                name: "Aksesuarlar",
                slug: "aksesuarlar",
                description: "Development üçün nümunə kateqoriya.",
                displayOrder: 3),
        };

        foreach (var r in roots)
            categories.Add(r);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var gulRoot = roots[0];
        var child = Category.Create(
            name: "Təzə kəsilmiş güllər",
            slug: "teze-kesilmis-guller",
            description: "Alt kateqoriya nümunəsi.",
            parentId: gulRoot.Id,
            displayOrder: 1);
        categories.Add(child);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DevCatalogSeed: {RootCount} kök və 1 alt kateqoriya yaradıldı (Development).",
            roots.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
