namespace Gulora.API.Configuration;

/// <summary>
/// Yalnız Development: bazada kateqoriya yoxdursa demo kataloq əlavə edir.
/// Prod-da bölmə olmamalı və ya <c>Enabled: false</c> olmalıdır.
/// </summary>
public sealed class DevCatalogSeedOptions
{
    public const string SectionName = "DevCatalogSeed";

    /// <summary>Seed işləsinmi.</summary>
    public bool Enabled { get; init; }
}
