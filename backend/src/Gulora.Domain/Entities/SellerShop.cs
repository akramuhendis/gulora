using Gulora.Domain.Common;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Satıcı mağaza vitrin bilgisi — Trendyol tarzı pazaryerinde her satıcının benzersiz slug ile mağaza sayfası.
/// <see cref="User"/> ile 1:1 ilişki (<see cref="UserId"/>).
/// </summary>
public sealed class SellerShop : BaseEntity
{
    public Guid UserId { get; private set; }
    public string StoreName { get; private set; } = string.Empty;
    public string StoreSlug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }

    private SellerShop() { }

    public static SellerShop Create(
        Guid userId,
        string storeName,
        string storeSlug,
        string? description = null,
        string? logoUrl = null,
        string? bannerUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storeSlug);

        return new SellerShop
        {
            UserId = userId,
            StoreName = storeName.Trim(),
            StoreSlug = NormalizeSlug(storeSlug),
            Description = description?.Trim(),
            LogoUrl = logoUrl?.Trim(),
            BannerUrl = bannerUrl?.Trim()
        };
    }

    public void Update(
        string storeName,
        string storeSlug,
        string? description,
        string? logoUrl,
        string? bannerUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storeSlug);

        StoreName = storeName.Trim();
        StoreSlug = NormalizeSlug(storeSlug);
        Description = description?.Trim();
        LogoUrl = logoUrl?.Trim();
        BannerUrl = bannerUrl?.Trim();
        MarkUpdated();
    }

    private static string NormalizeSlug(string slug)
    {
        var s = slug.Trim().ToLowerInvariant();
        if (s.Length == 0)
            throw new BusinessRuleException("Mağaza adresi (slug) boş olamaz.");
        return s;
    }
}
