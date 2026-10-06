using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.SellerShops;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerShops.Queries.GetMySellerShop;

public sealed class GetMySellerShopQueryHandler
    : IRequestHandler<GetMySellerShopQuery, SellerShopMyResponse?>
{
    private readonly ISellerShopRepository _sellerShopRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMySellerShopQueryHandler(
        ISellerShopRepository sellerShopRepository,
        ICurrentUserService currentUser)
    {
        _sellerShopRepository = sellerShopRepository;
        _currentUser = currentUser;
    }

    public async Task<SellerShopMyResponse?> Handle(GetMySellerShopQuery _, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedException();

        if (!string.Equals(_currentUser.Role, "Seller", StringComparison.Ordinal))
            throw new ForbiddenException("Bu bilgi yalnızca satıcı hesapları içindir.");

        var shop = await _sellerShopRepository.GetByUserIdAsync(_currentUser.UserId.Value, ct);
        if (shop is null)
            return null;

        return new SellerShopMyResponse(
            shop.UserId,
            shop.StoreName,
            shop.StoreSlug,
            shop.Description,
            shop.LogoUrl,
            shop.BannerUrl,
            shop.CreatedAt,
            shop.UpdatedAt);
    }
}
