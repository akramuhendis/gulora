using Gulora.Contracts.Wishlist;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Wishlist.Queries.GetWishlist;

public sealed class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistResponse>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;

    public GetWishlistQueryHandler(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
    }

    public async Task<WishlistResponse> Handle(GetWishlistQuery query, CancellationToken ct)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(query.UserId, ct);
        if (wishlist is null)
            return new WishlistResponse(Guid.Empty, query.UserId, 0, Array.Empty<WishlistItemResponse>());

        var items = new List<WishlistItemResponse>();
        foreach (var item in wishlist.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            if (product is null) continue;

            items.Add(new WishlistItemResponse(
                item.Id,
                item.ProductId,
                product.Name,
                product.Slug,
                product.EffectivePrice,
                product.PrimaryImageUrl,
                product.IsAvailableForPurchase,
                item.CreatedAt));
        }

        return new WishlistResponse(wishlist.Id, wishlist.UserId, wishlist.Count, items.AsReadOnly());
    }
}
