using Gulora.Application.Common.Authorization;
using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Commands.ActivateProduct;

public sealed class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ActivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateProductCommand cmd, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new NotFoundException("Ürün", cmd.ProductId);

        ProductMutationAuthorization.EnsureCanModifyProduct(_currentUser, product);

        product.Activate();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
