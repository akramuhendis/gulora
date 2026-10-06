using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProductBySlug;

/// <summary>URL-dostu slug ile ürün detayını getirir.</summary>
public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailResponse>;
