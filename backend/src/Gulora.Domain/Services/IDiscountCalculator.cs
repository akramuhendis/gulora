using Gulora.Domain.Entities;
using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Services;

/// <summary>
/// Sipariş veya sepet üzerinde indirim hesaplaması yapan domain servis arayüzü.
/// İş kuralı Domain katmanında kalır; kalıcılık Infrastructure'dadır.
/// </summary>
public interface IDiscountCalculator
{
    /// <summary>
    /// Kuponu siparişe uygulayarak indirim tutarını hesaplar.
    /// Kupon geçersizse veya koşulları sağlanmıyorsa <see cref="Exceptions.BusinessRuleException"/> fırlatır.
    /// </summary>
    /// <param name="coupon">Uygulanacak kupon varlığı.</param>
    /// <param name="orderTotal">Kupon öncesi sipariş toplamı.</param>
    /// <param name="userId">Kullanan kullanıcı (kullanım limiti kontrolü için).</param>
    /// <returns>İndirim tutarını döner (<paramref name="orderTotal"/> ile kısıtlı).</returns>
    Task<Money> CalculateDiscountAsync(
        DiscountCoupon coupon,
        Money orderTotal,
        Guid userId,
        CancellationToken ct = default);
}
