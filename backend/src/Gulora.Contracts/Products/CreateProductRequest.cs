namespace Gulora.Contracts.Products;

/// <summary>
/// POST api/v1/products gövdesi (Admin / Seller).
/// SellerId bu kontratta yer almaz — Application handler kimliği JWT claim'inden türetir.
/// DiscountedPrice ve IsFeatured ürün oluşturulurken isteğe bağlı olarak uygulanır;
/// sonradan PATCH /products/{id}/price ve PUT /products/{id} ile de güncellenebilir.
/// </summary>
public sealed record CreateProductRequest(
    string Name,
    string Slug,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    string? Description = null,
    string? ShortDescription = null,
    string? Sku = null,
    string? Brand = null,
    decimal? DiscountedPrice = null,
    bool IsFeatured = false);
