using MediatR;

namespace Gulora.Application.Wishlist.Commands.AddToWishlist;

/// <summary>Ürünü kullanıcının istek listesine ekler.</summary>
public sealed record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest;
