using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.SellerShops;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerShops.Queries.GetSellerShopBySlug;

public sealed class GetSellerShopBySlugQueryHandler
    : IRequestHandler<GetSellerShopBySlugQuery, SellerShopPublicResponse>
{
    private readonly ISellerShopRepository _sellerShopRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public GetSellerShopBySlugQueryHandler(
        ISellerShopRepository sellerShopRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _sellerShopRepository = sellerShopRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task<SellerShopPublicResponse> Handle(
        GetSellerShopBySlugQuery query,
        CancellationToken ct)
    {
        var slug = query.Slug.Trim();
        var shop = await _sellerShopRepository.GetBySlugAsync(slug, ct)
            ?? throw new NotFoundException("Mağaza bulunamadı.");

        var seller = await _userRepository.GetByIdAsync(shop.UserId, ct)
            ?? throw new NotFoundException("Mağaza bulunamadı.");

        if (!seller.IsActive || seller.Role != UserRole.Seller)
            throw new NotFoundException("Mağaza bulunamadı.");

        var productCount = await _productRepository.CountActiveCatalogBySellerAsync(shop.UserId, ct);

        return new SellerShopPublicResponse(
            shop.UserId,
            shop.StoreName,
            shop.StoreSlug,
            shop.Description,
            shop.LogoUrl,
            shop.BannerUrl,
            seller.FullName,
            productCount,
            shop.CreatedAt);
    }
}
