using Gulora.Contracts.SellerShops;
using MediatR;

namespace Gulora.Application.SellerShops.Queries.GetSellerShopBySlug;

public sealed record GetSellerShopBySlugQuery(string Slug) : IRequest<SellerShopPublicResponse>;
