namespace Gulora.Contracts.Wishlist;

/// <summary>POST api/v1/wishlist gövdesi.</summary>
public sealed record AddToWishlistRequest(
    Guid ProductId);
