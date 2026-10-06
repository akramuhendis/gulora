namespace Gulora.Contracts.Products;

/// <summary>PUT api/v1/products/{id} gövdesi (Admin / Seller).</summary>
public sealed record UpdateProductRequest(
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    string? Brand,
    Guid CategoryId,
    bool IsFeatured);
