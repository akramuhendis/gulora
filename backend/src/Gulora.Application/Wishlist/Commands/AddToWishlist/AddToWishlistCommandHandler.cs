using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Wishlist.Commands.AddToWishlist;

public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddToWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddToWishlistCommand cmd, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new NotFoundException("Ürün", cmd.ProductId);

        var wishlist = await _wishlistRepository.GetOrCreateAsync(cmd.UserId, ct);
        if (wishlist.Contains(cmd.ProductId))
            return;

        wishlist.AddProduct(cmd.ProductId);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
