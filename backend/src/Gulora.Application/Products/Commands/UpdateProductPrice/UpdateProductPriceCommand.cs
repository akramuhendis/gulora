using MediatR;

namespace Gulora.Application.Products.Commands.UpdateProductPrice;

/// <summary>Ürün fiyatını ve opsiyonel indirimli fiyatı günceller.</summary>
public sealed record UpdateProductPriceCommand(
    Guid ProductId,
    decimal NewPrice,
    decimal? DiscountedPrice = null
) : IRequest;
