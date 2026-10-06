using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>Satıcı mağaza vitrin kayıtları.</summary>
public interface ISellerShopRepository : IRepository<SellerShop>
{
    Task<SellerShop?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<SellerShop?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Slug başka bir mağaza tarafından kullanılıyor mu?
    /// <paramref name="excludeUserId"/> verilirse bu satıcının kendi slug'ı sayılmaz.
    /// </summary>
    Task<bool> IsSlugTakenByOtherSellerAsync(
        string slug,
        Guid? excludeUserId = null,
        CancellationToken ct = default);
}
