namespace Gulora.Contracts.Wishlist;

/// <summary>İstek listesi yanıtı — GET api/v1/wishlist.</summary>
public sealed record WishlistResponse(
    Guid Id,
    Guid UserId,
    int ItemCount,
    IReadOnlyList<WishlistItemResponse> Items);

/// <summary>İstek listesi ürün satırı.</summary>
public sealed record WishlistItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    decimal EffectivePrice,
    string? PrimaryImageUrl,
    bool IsAvailable,
    DateTime AddedAt);
