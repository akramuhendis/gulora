using Gulora.API.Configuration;
using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Gulora.API.Services;

/// <summary>
/// Development ortamında təyin olunmuş e-poçt ilə Seller rolündə istifadəçi yoxdursa yaradır;
/// satıcı mövcuddursa mağaza (seller_shops) qeydi yoxdursa əlavə edir.
/// </summary>
public sealed class DevSellerSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevSellerSeedHostedService> _logger;
    private readonly DevSellerSeedOptions _options;

    public DevSellerSeedHostedService(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<DevSellerSeedHostedService> logger,
        IOptions<DevSellerSeedOptions> options)
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

        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogWarning("DevSellerSeed: Email və ya Password boşdur — seed atlanır.");
            return;
        }

        try
        {
            await RunSeedAsync(cancellationToken);
        }
        catch (Exception ex) when (IsMissingSellerShopsTable(ex))
        {
            _logger.LogWarning(
                ex,
                "DevSellerSeed: \"seller_shops\" cədvəli veritabanında yoxdur. Migration tətbiq edin: " +
                "dotnet ef database update --project backend/src/Gulora.Infrastructure --startup-project backend/src/Gulora.API. " +
                "Satıcı seed və mağaza yoxlaması atlandı.");
        }
    }

    private async Task RunSeedAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var sellerShopRepository = scope.ServiceProvider.GetRequiredService<ISellerShopRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = _options.Email.Trim();

        var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is null)
        {
            var hash = passwordHasher.Hash(_options.Password);
            var user = User.Create(
                _options.FirstName,
                _options.LastName,
                email,
                hash,
                UserRole.Seller);

            userRepository.Add(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await EnsureShopForSellerAsync(
                sellerShopRepository,
                unitOfWork,
                user.Id,
                $"{_options.FirstName} {_options.LastName}".Trim(),
                email,
                cancellationToken);

            _logger.LogInformation(
                "DevSellerSeed: demo satıcı yaradıldı — {Email} (rol: Seller). Parola appsettings.Development.json içindədir.",
                email);
            return;
        }

        if (existingUser.Role != UserRole.Seller)
        {
            _logger.LogInformation(
                "DevSellerSeed: \"{Email}\" mövcuddur amma Seller deyil — mağaza toxumu atlanır.",
                email);
            return;
        }

        await EnsureShopForSellerAsync(
            sellerShopRepository,
            unitOfWork,
            existingUser.Id,
            existingUser.FullName,
            email,
            cancellationToken);

        _logger.LogInformation(
            "DevSellerSeed: \"{Email}\" üçün mağaza qeydi yoxlanıldı.",
            email);
    }

    /// <summary>
    /// Migration tətbiq olunmayıbsa Npgsql 42P01 (undefined_table) — API layihəsinə Npgsql əlavə etmədən
    /// istisna zəncirindən tanınır.
    /// </summary>
    private static bool IsMissingSellerShopsTable(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException!)
        {
            if (e.Message.Contains("seller_shops", StringComparison.OrdinalIgnoreCase)
                && e.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private async Task EnsureShopForSellerAsync(
        ISellerShopRepository sellerShopRepository,
        IUnitOfWork unitOfWork,
        Guid sellerUserId,
        string displayName,
        string email,
        CancellationToken ct)
    {
        if (await sellerShopRepository.GetByUserIdAsync(sellerUserId, ct) is not null)
            return;

        var baseSlug = SlugFromEmailOrName(email, displayName);
        var slug = baseSlug;
        var n = 0;
        while (await sellerShopRepository.IsSlugTakenByOtherSellerAsync(slug, null, ct))
        {
            n++;
            slug = $"{baseSlug}-{n}";
            if (slug.Length > 120)
                slug = slug[..120];
        }

        var shop = SellerShop.Create(
            sellerUserId,
            storeName: string.IsNullOrWhiteSpace(displayName) ? "Demo Mağaza" : displayName.Trim(),
            storeSlug: slug,
            description: "Development üçün nümunə satıcı mağazası.",
            logoUrl: null,
            bannerUrl: null);

        sellerShopRepository.Add(shop);
        await unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DevSellerSeed: mağaza yaradıldı — slug: {Slug}",
            slug);
    }

    /// <summary>E-poçt yereli hissəsindən və ya göstərilən addan URL təhlükəsiz slug yaradır.</summary>
    private static string SlugFromEmailOrName(string email, string displayName)
    {
        var source = email.Split('@')[0].Trim();
        if (source.Length < 2)
            source = displayName.Trim().ToLowerInvariant();

        Span<char> buf = stackalloc char[Math.Min(source.Length, 128)];
        var j = 0;
        var prevDash = false;
        foreach (var ch in source.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(ch))
            {
                buf[j++] = ch;
                prevDash = false;
            }
            else if ((ch is '.' or '_' or '-' or ' ') && j > 0 && !prevDash)
            {
                buf[j++] = '-';
                prevDash = true;
            }

            if (j >= buf.Length)
                break;
        }

        while (j > 0 && buf[j - 1] == '-')
            j--;

        var s = j > 0 ? new string(buf[..j]) : "magaza";

        if (s.Length < 3)
            s = $"magaza-{Guid.NewGuid().ToString("N")[..12]}";

        return s.Length > 120 ? s[..120] : s;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
