using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// İndirim kuponu sorgu operasyonları.
/// </summary>
public interface IDiscountCouponRepository : IRepository<DiscountCoupon>
{
    /// <summary>Kupon kodu ile kuponu getirir (büyük/küçük harf duyarsız).</summary>
    Task<DiscountCoupon?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Kullanıcının bu kuponu daha önce kullanıp kullanmadığını kontrol eder.</summary>
    Task<bool> HasUserUsedCouponAsync(Guid couponId, Guid userId, CancellationToken ct = default);
}
