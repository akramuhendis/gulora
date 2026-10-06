using Gulora.Application.Common.Authorization;
using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        if (await _productRepository.IsSlugTakenAsync(cmd.Slug, null, ct))
            throw new ConflictException($"'{cmd.Slug}' slug'ı zaten kullanılıyor.");

        var category = await _categoryRepository.GetByIdAsync(cmd.CategoryId, ct)
            ?? throw new NotFoundException("Kategori", cmd.CategoryId);

        var sellerId = ProductMutationAuthorization.ResolveSellerIdForCreate(_currentUser);

        var product = Product.Create(
            cmd.Name,
            cmd.Slug,
            cmd.Price,
            cmd.StockQuantity,
            cmd.CategoryId,
            cmd.Description,
            cmd.ShortDescription,
            cmd.Sku,
            cmd.Brand,
            sellerId);

        if (cmd.DiscountedPrice.HasValue || cmd.IsFeatured)
            product.UpdateDetails(
                product.Name,
                product.Slug,
                product.Description,
                product.ShortDescription,
                product.Brand,
                product.CategoryId,
                cmd.IsFeatured);

        if (cmd.DiscountedPrice.HasValue)
            product.UpdatePrice(product.Price, cmd.DiscountedPrice);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return product.Id;
    }
}
