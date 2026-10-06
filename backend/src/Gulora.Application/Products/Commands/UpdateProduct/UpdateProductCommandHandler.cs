using Gulora.Application.Common.Authorization;
using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new NotFoundException("Ürün", cmd.ProductId);

        ProductMutationAuthorization.EnsureCanModifyProduct(_currentUser, product);

        if (await _productRepository.IsSlugTakenAsync(cmd.Slug, cmd.ProductId, ct))
            throw new ConflictException($"'{cmd.Slug}' slug'ı başka bir ürün tarafından kullanılıyor.");

        product.UpdateDetails(
            cmd.Name,
            cmd.Slug,
            cmd.Description,
            cmd.ShortDescription,
            cmd.Brand,
            cmd.CategoryId,
            cmd.IsFeatured);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
