using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.SellerShops;
using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerShops.Commands.UpsertMySellerShop;

public sealed class UpsertMySellerShopCommandHandler
    : IRequestHandler<UpsertMySellerShopCommand, SellerShopMyResponse>
{
    private readonly ISellerShopRepository _sellerShopRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpsertMySellerShopCommandHandler(
        ISellerShopRepository sellerShopRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _sellerShopRepository = sellerShopRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SellerShopMyResponse> Handle(UpsertMySellerShopCommand cmd, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedException();

        if (!string.Equals(_currentUser.Role, "Seller", StringComparison.Ordinal))
            throw new ForbiddenException("Mağaza bilgisi yalnızca satıcı hesapları tarafından güncellenebilir.");

        var userId = _currentUser.UserId.Value;

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new UnauthorizedException();

        if (user.Role != UserRole.Seller || !user.IsActive)
            throw new ForbiddenException("Mağaza bilgisi yalnızca aktif satıcı hesapları tarafından güncellenebilir.");

        if (await _sellerShopRepository.IsSlugTakenByOtherSellerAsync(cmd.StoreSlug, userId, ct))
            throw new ConflictException($"'{cmd.StoreSlug.Trim()}' mağaza adresi başka bir satıcı tarafından kullanılıyor.");

        var existing = await _sellerShopRepository.GetByUserIdAsync(userId, ct);
        if (existing is null)
        {
            var shop = SellerShop.Create(
                userId,
                cmd.StoreName,
                cmd.StoreSlug,
                cmd.Description,
                cmd.LogoUrl,
                cmd.BannerUrl);
            _sellerShopRepository.Add(shop);
        }
        else
        {
            existing.Update(
                cmd.StoreName,
                cmd.StoreSlug,
                cmd.Description,
                cmd.LogoUrl,
                cmd.BannerUrl);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _sellerShopRepository.GetByUserIdAsync(userId, ct)
            ?? throw new BusinessRuleException("Mağaza kaydı oluşturulamadı.");

        return new SellerShopMyResponse(
            result.UserId,
            result.StoreName,
            result.StoreSlug,
            result.Description,
            result.LogoUrl,
            result.BannerUrl,
            result.CreatedAt,
            result.UpdatedAt);
    }
}
