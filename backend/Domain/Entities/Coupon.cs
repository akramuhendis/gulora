using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Sepet / sipariş toplamına uygulanacak indirim kuralı.
/// Yüzde veya sabit tutar; minimum sepet, kullanım limiti ve son kullanma tarihi ile sınırlandırılabilir.
/// </summary>
public class Coupon : BaseEntity
{
    /// <summary>Kupon kodu; depoda büyük harfe normalize edilir.</summary>
    public string Code { get; private set; } = default!;
    /// <summary><see cref="Enums.DiscountType.Percent"/> veya <see cref="Enums.DiscountType.Fixed"/>.</summary>
    public DiscountType DiscountType { get; private set; }
    /// <summary>Yüzde ise 0–100 arası anlamlı; sabit ise para birimi tutarı.</summary>
    public decimal DiscountValue { get; private set; }
    /// <summary>Bu tutarın altındaki sepetlerde kupon geçersiz sayılır.</summary>
    public decimal MinOrderAmount { get; private set; }
    /// <summary>Toplam kaç kez kullanılabilir; null sınırsız anlamına gelebilir (iş kuralına göre).</summary>
    public int? UsageLimit { get; private set; }
    /// <summary>Şu ana kadar kaç kez kullanıldı.</summary>
    public int UsedCount { get; private set; }
    /// <summary>Son geçerlilik tarihi (UTC); null süresiz olabilir.</summary>
    public DateTime? ExpiresAt { get; private set; }
    /// <summary>Yönetici tarafından pasifleştirilmiş kuponlar false.</summary>
    public bool IsActive { get; private set; } = true;

    private Coupon() { }

    /// <summary>Yeni kupon tanımı oluşturur.</summary>
    /// <exception cref="DomainException">Yüzde tipinde değer 100’ü aşarsa.</exception>
    public static Coupon Create(string code, DiscountType type, decimal value,
        decimal minOrderAmount = 0, int? usageLimit = null, DateTime? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        ArgumentOutOfRangeException.ThrowIfNegative(minOrderAmount);
        if (type == DiscountType.Percent && value > 100)
            throw new DomainException("Faiz endirimi 100-dən böyük ola bilməz.");

        var coupon = new Coupon
        {
            Code = code.ToUpperInvariant(), DiscountType = type,
            DiscountValue = value, MinOrderAmount = minOrderAmount,
            UsageLimit = usageLimit, ExpiresAt = expiresAt
        };
        coupon.RegisterDomainEvent(new CouponCreatedDomainEvent(coupon.Id, coupon.Code));
        return coupon;
    }

    /// <summary>Verilen sepet tutarı için kupon şartlarının hepsi sağlanıyorsa true.</summary>
    /// <param name="orderAmount">İndirim öncesi uygun tutar (genelde ara toplam).</param>
    public bool IsValid(decimal orderAmount)
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value) return false;
        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) return false;
        if (orderAmount < MinOrderAmount) return false;
        return true;
    }

    /// <summary>Geçerli kupona göre indirim tutarını hesaplar.</summary>
    /// <returns>Para birimi cinsinden indirim (sepet tutarını aşmaz).</returns>
    /// <exception cref="DomainException">Kupon geçerli değilse.</exception>
    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (!IsValid(orderAmount)) throw new DomainException("Kupon etibarsızdır.");

        return DiscountType == DiscountType.Percent
            ? Math.Round(orderAmount * DiscountValue / 100, 2)
            : Math.Min(DiscountValue, orderAmount);
    }

    /// <summary>Başarılı sipariş sonrası kullanım sayacını artırır.</summary>
    /// <exception cref="DomainException">Kullanım limiti dolmuşsa.</exception>
    public void IncrementUsage()
    {
        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
            throw new DomainException("Kupon istifadə limiti dolub.");
        UsedCount++;
        SetUpdated();
    }

    /// <summary>Sifariş ləğv / uğursuz ödeme sonrası sayğacı geri alır.</summary>
    public void DecrementUsage()
    {
        if (UsedCount <= 0)
            throw new DomainException("Kupon istifadə sayğacı artıq sıfırdır.");
        UsedCount--;
        SetUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        SetUpdated();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        SetUpdated();
    }

    /// <summary>Admin: endirim şərtlərini yeniləyir (kod dəyişmir).</summary>
    public void UpdateDiscountTerms(DiscountType discountType, decimal discountValue,
        decimal minOrderAmount, int? usageLimit, DateTime? expiresAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(discountValue);
        ArgumentOutOfRangeException.ThrowIfNegative(minOrderAmount);
        if (discountType == DiscountType.Percent && discountValue > 100)
            throw new DomainException("Faiz endirimi 100-dən böyük ola bilməz.");
        if (usageLimit is int ul && ul < UsedCount)
            throw new DomainException("İstifadə limiti mövcud istifadə sayından kiçik ola bilməz.");

        DiscountType = discountType;
        DiscountValue = discountValue;
        MinOrderAmount = minOrderAmount;
        UsageLimit = usageLimit;
        ExpiresAt = expiresAt;
        SetUpdated();
    }
}
