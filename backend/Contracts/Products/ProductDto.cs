namespace Gulora.Contracts.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    decimal? OriginalPrice,
    int? DiscountPercent,
    decimal Rating,
    int ReviewCount,
    string ImageUrl,
    List<string> Images,
    string CategorySlug,
    string CategoryName,
    List<string> Tags,
    string? Badge,
    bool IsSameDay,
    bool IsFreeDelivery,
    string? Description,
    int Stock);
