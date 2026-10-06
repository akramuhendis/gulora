using Gulora.Contracts.SellerShops;
using MediatR;

namespace Gulora.Application.SellerShops.Commands.UpsertMySellerShop;

public sealed record UpsertMySellerShopCommand(
    string StoreName,
    string StoreSlug,
    string? Description,
    string? LogoUrl,
    string? BannerUrl) : IRequest<SellerShopMyResponse>;
