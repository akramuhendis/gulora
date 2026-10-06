namespace Gulora.Contracts.Products;

/// <summary>PATCH api/v1/products/{id}/stock gövdesi.</summary>
public sealed record UpdateProductStockRequest(int StockQuantity);
