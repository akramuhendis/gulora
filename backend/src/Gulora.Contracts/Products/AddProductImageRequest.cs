namespace Gulora.Contracts.Products;

/// <summary>POST api/v1/products/{id}/images gövdesi.</summary>
public sealed record AddProductImageRequest(
    string Url,
    string? AltText = null,
    int DisplayOrder = 0);
