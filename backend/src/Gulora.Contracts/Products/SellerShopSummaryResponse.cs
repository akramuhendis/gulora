namespace Gulora.Contracts.Products;

/// <summary>Ürün detayında mağaza bağlantısı — müşteri mağaza sayfasına yönlenebilir.</summary>
public sealed record SellerShopSummaryResponse(
    Guid SellerUserId,
    string StoreName,
    string StoreSlug,
    string? LogoUrl);
