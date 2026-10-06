using Gulora.Application.Common.Authorization;
using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Commands.UpdateProductStock;

public sealed class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductStockCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProductStockCommand cmd, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new NotFoundException("Ürün", cmd.ProductId);

        ProductMutationAuthorization.EnsureCanModifyProduct(_currentUser, product);

        var diff = cmd.StockQuantity - product.StockQuantity;
        if (diff > 0)
            product.IncrementStock(diff);
        else if (diff < 0)
            product.DecrementStock(-diff);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
