using Gulora.Contracts.SellerShops;
using MediatR;

namespace Gulora.Application.SellerShops.Queries.GetMySellerShop;

public sealed record GetMySellerShopQuery : IRequest<SellerShopMyResponse?>;
