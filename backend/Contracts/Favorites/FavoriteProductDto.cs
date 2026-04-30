using Gulora.Contracts.Products;

namespace Gulora.Contracts.Favorites;

public record FavoriteProductDto(Guid FavoriteId, DateTime AddedAt, ProductDto Product);
