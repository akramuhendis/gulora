using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// İndirim kuponu aggregate root'u.
/// Kupon kodu büyük harfli ve benzersiz; kullanım limiti ve süre kontrolü bu aggregate'de yapılır.
/// Hesaplama mantığı <see cref="Gulora.Domain.Services.IDiscountCalculator"/> arayüzündedir.
/// </summary>
public sealed class DiscountCoupon : BaseAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; }
    public decimal? MinOrderAmount { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int UsageCount { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? RestrictedUserId { get; private set; }

    private DiscountCoupon() { }

    public static DiscountCoupon Create(
        string code,
        DiscountType type,
        decimal value,
        string? description = null,
        decimal? minOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        DateTime? startsAt = null,
        DateTime? expiresAt = null,
        Guid? restrictedUserId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        if (value <= 0)
            throw new BusinessRuleException("İndirim değeri sıfırdan büyük olmalıdır.");

        if (type == DiscountType.Percent && value > 100)
            throw new BusinessRuleException("Yüzde indirim 100'den fazla olamaz.");

        if (expiresAt.HasValue && startsAt.HasValue && expiresAt <= startsAt)
            throw new BusinessRuleException("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");

        return new DiscountCoupon
        {
            Code = code.Trim().ToUpperInvariant(),
            Type = type,
            Value = value,
            Description = description?.Trim(),
            MinOrderAmount = minOrderAmount,
            MaxDiscountAmount = maxDiscountAmount,
            UsageLimit = usageLimit,
            StartsAt = startsAt,
            ExpiresAt = expiresAt,
            IsActive = true,
            RestrictedUserId = restrictedUserId
        };
    }

    /// <summary>Kuponun belirtilen anda geçerli olup olmadığını kontrol eder.</summary>
    public bool IsValidAt(DateTime utcNow)
    {
        if (!IsActive) return false;
        if (StartsAt.HasValue && utcNow < StartsAt.Value) return false;
        if (ExpiresAt.HasValue && utcNow > ExpiresAt.Value) return false;
        if (UsageLimit.HasValue && UsageCount >= UsageLimit.Value) return false;
        return true;
    }

    /// <summary>
    /// Kuponu belirtilen kullanıcı ve sipariş tutarı için geçerli mi doğrular.
    /// Geçersizse <see cref="BusinessRuleException"/> fırlatır.
    /// </summary>
    public void ValidateFor(Guid userId, decimal orderAmount, DateTime utcNow)
    {
        if (!IsValidAt(utcNow))
            throw new BusinessRuleException("Kupon geçerli değil veya süresi dolmuş.", "COUPON_INVALID");

        if (RestrictedUserId.HasValue && RestrictedUserId.Value != userId)
            throw new ForbiddenException("Bu kupon sizin için geçerli değil.");

        if (MinOrderAmount.HasValue && orderAmount < MinOrderAmount.Value)
            throw new BusinessRuleException(
                $"Bu kuponu kullanmak için minimum sipariş tutarı {MinOrderAmount.Value:F2} AZN olmalıdır.",
                "COUPON_MIN_AMOUNT");
    }

    /// <summary>Kullanım sayısını bir artırır.</summary>
    public void IncrementUsage()
    {
        UsageCount++;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    /// <summary>
    /// Kupon kodu, türü ve tutarı değiştirilmeden meta veri ve geçerlilik alanlarını günceller.
    /// </summary>
    public void UpdateMetadata(
        string? description,
        decimal? minOrderAmount,
        decimal? maxDiscountAmount,
        int? usageLimit,
        DateTime? startsAt,
        DateTime? expiresAt,
        bool isActive)
    {
        if (expiresAt.HasValue && startsAt.HasValue && expiresAt <= startsAt)
            throw new BusinessRuleException("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");

        Description = description?.Trim();
        MinOrderAmount = minOrderAmount;
        MaxDiscountAmount = maxDiscountAmount;
        UsageLimit = usageLimit;
        StartsAt = startsAt;
        ExpiresAt = expiresAt;
        IsActive = isActive;
        MarkUpdated();
    }
}
