using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// İstek listesi sorgu operasyonları.
/// </summary>
public interface IWishlistRepository : IRepository<Wishlist>
{
    /// <summary>Kullanıcının istek listesini ürünleriyle birlikte getirir.</summary>
    Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Kullanıcının istek listesi yoksa oluşturur, varsa getirir.</summary>
    Task<Wishlist> GetOrCreateAsync(Guid userId, CancellationToken ct = default);
}
