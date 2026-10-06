using AutoMapper;
using Gulora.Contracts.Products;
using Gulora.Domain.Entities;

namespace Gulora.Application.Products.Queries.GetProductBySlug;

public sealed class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDetailResponse>()
            .ConvertUsing(product => new ProductDetailResponse(
                product.Id,
                product.Name,
                product.Slug,
                product.Description,
                product.ShortDescription,
                product.Price,
                product.DiscountedPrice,
                product.EffectivePrice,
                product.StockQuantity,
                product.IsAvailableForPurchase,
                product.Sku,
                product.Brand,
                product.CategoryId,
                string.Empty,
                product.SellerId,
                product.IsActive,
                product.IsFeatured,
                product.AverageRating,
                product.ReviewCount,
                product.PrimaryImageUrl,
                product.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageResponse(i.Id, i.Url, i.AltText, i.DisplayOrder, i.IsActive))
                    .ToList(),
                product.CreatedAt,
                product.UpdatedAt,
                null,
                null));
    }
}
