namespace Gulora.Contracts.Products;

/// <summary>Ürün görsel yanıtı.</summary>
public sealed record ProductImageResponse(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsActive);
