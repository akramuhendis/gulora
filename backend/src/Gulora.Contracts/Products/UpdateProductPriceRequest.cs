namespace Gulora.Contracts.Products;

/// <summary>PATCH api/v1/products/{id}/price gövdesi.</summary>
public sealed record UpdateProductPriceRequest(
    decimal Price,
    decimal? DiscountedPrice = null);
