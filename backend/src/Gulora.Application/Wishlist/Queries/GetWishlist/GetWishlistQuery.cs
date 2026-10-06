using Gulora.Contracts.Wishlist;
using MediatR;

namespace Gulora.Application.Wishlist.Queries.GetWishlist;

/// <summary>Kullanıcının istek listesini ürün detaylarıyla birlikte getirir.</summary>
public sealed record GetWishlistQuery(Guid UserId) : IRequest<WishlistResponse>;
