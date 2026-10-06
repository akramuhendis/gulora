namespace Gulora.Contracts.Products;

/// <summary>
/// Ürün listesi kartı — GET api/v1/products (liste/arama).
/// Ağır alanlar (tam açıklama, tüm görseller) dahil değildir.
/// </summary>
public sealed record ProductSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    decimal Price,
    decimal? DiscountedPrice,
    decimal EffectivePrice,
    int StockQuantity,
    bool IsActive,
    bool IsAvailableForPurchase,
    string? Brand,
    Guid CategoryId,
    string CategoryName,
    bool IsFeatured,
    double AverageRating,
    int ReviewCount,
    string? PrimaryImageUrl,
    DateTime CreatedAt,
    ProductSellerCampaignOffer? SellerCampaign);
