namespace Gulora.Contracts.SellerShops;

/// <summary>Mağaza vitrin sayfası — GET api/v1/seller-shops/{slug}</summary>
public sealed record SellerShopPublicResponse(
    Guid SellerUserId,
    string StoreName,
    string StoreSlug,
    string? Description,
    string? LogoUrl,
    string? BannerUrl,
    string? OwnerDisplayName,
    int ActiveProductCount,
    DateTime CreatedAt);

/// <summary>Satıcı paneli — GET api/v1/seller-shops/me</summary>
public sealed record SellerShopMyResponse(
    Guid SellerUserId,
    string StoreName,
    string StoreSlug,
    string? Description,
    string? LogoUrl,
    string? BannerUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>PUT api/v1/seller-shops/me</summary>
public sealed record UpsertSellerShopRequest(
    string StoreName,
    string StoreSlug,
    string? Description,
    string? LogoUrl,
    string? BannerUrl);
