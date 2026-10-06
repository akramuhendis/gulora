using MediatR;

namespace Gulora.Application.Products.Commands.UpdateProductStock;

/// <summary>Ürün stok miktarını mutlak değer olarak günceller.</summary>
public sealed record UpdateProductStockCommand(
    Guid ProductId,
    int StockQuantity
) : IRequest;
