using Gulora.Application.Common.Authorization;
using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Commands.AddProductImage;

public sealed class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AddProductImageCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddProductImageCommand cmd, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new Gulora.Domain.Exceptions.NotFoundException("Ürün", cmd.ProductId);

        ProductMutationAuthorization.EnsureCanModifyProduct(_currentUser, product);

        var image = ProductImage.Create(cmd.ProductId, cmd.Url, cmd.AltText, cmd.DisplayOrder);
        product.AddImage(image);

        await _unitOfWork.SaveChangesAsync(ct);

        return image.Id;
    }
}
