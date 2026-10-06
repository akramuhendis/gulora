using MediatR;

namespace Gulora.Application.Products.Commands.CreateProduct;

/// <summary>Yeni ürün oluşturur. Başarıda ürün Guid'i döner.</summary>
public sealed record CreateProductCommand(
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
    bool IsFeatured = false
) : IRequest<Guid>;
