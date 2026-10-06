namespace Gulora.API.Configuration;

/// <summary>
/// Yalnızca Development ortamında çalışan demo satıcı hesabı (IHostedService seed).
/// Üretimde bu bölüm tanımlı olmamalı veya Enabled=false olmalıdır.
/// </summary>
public sealed class DevSellerSeedOptions
{
    public const string SectionName = "DevSellerSeed";

    /// <summary>Seed çalışsın mı (varsayılan: Development appsettings içinde true).</summary>
    public bool Enabled { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FirstName { get; init; } = "Demo";

    public string LastName { get; init; } = "Satıcı";
}
