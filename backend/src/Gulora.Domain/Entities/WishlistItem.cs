using Gulora.Domain.Common;

namespace Gulora.Domain.Entities;

/// <summary>
/// İstek listesi kalemi — Wishlist aggregate'inin alt entity'sidir.
/// </summary>
public sealed class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Guid ProductId { get; private set; }

    private WishlistItem() { }

    public static WishlistItem Create(Guid wishlistId, Guid productId)
    {
        return new WishlistItem
        {
            WishlistId = wishlistId,
            ProductId = productId
        };
    }
}
