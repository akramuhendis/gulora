using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Wishlist.Commands.RemoveFromWishlist;

public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromWishlistCommandHandler(IWishlistRepository wishlistRepository, IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveFromWishlistCommand cmd, CancellationToken ct)
    {
        var wishlist = await _wishlistRepository.GetByUserIdAsync(cmd.UserId, ct);
        if (wishlist is null) return;

        wishlist.RemoveProduct(cmd.ProductId);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
