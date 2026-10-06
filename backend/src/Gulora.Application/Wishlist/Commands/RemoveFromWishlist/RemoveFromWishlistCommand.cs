using MediatR;

namespace Gulora.Application.Wishlist.Commands.RemoveFromWishlist;

/// <summary>Ürünü kullanıcının istek listesinden kaldırır.</summary>
public sealed record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest;
