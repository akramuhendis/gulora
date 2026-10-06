namespace Gulora.Contracts.Products;

/// <summary>
/// Ürün detay sayfası — GET api/v1/products/{slug}.
/// Tüm alanlar, görseller ve değerlendirme özeti dahildir.
/// </summary>
public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    decimal Price,
    decimal? DiscountedPrice,
    decimal EffectivePrice,
    int StockQuantity,
    bool IsAvailableForPurchase,
    string? Sku,
    string? Brand,
    Guid CategoryId,
    string CategoryName,
    Guid? SellerId,
    bool IsActive,
    bool IsFeatured,
    double AverageRating,
    int ReviewCount,
    string? PrimaryImageUrl,
    IReadOnlyList<ProductImageResponse> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    SellerShopSummaryResponse? SellerShop,
    ProductSellerCampaignOffer? SellerCampaign);
