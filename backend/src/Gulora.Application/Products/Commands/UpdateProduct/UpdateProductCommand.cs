using MediatR;

namespace Gulora.Application.Products.Commands.UpdateProduct;

/// <summary>Ürün bilgilerini günceller.</summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Slug,
    string? Description,
    string? ShortDescription,
    string? Brand,
    Guid CategoryId,
    bool IsFeatured
) : IRequest;
