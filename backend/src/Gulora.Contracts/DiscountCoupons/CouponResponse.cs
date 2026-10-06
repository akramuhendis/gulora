namespace Gulora.Contracts.DiscountCoupons;

/// <summary>
/// İndirim kuponu yanıtı (Admin paneli).
/// Hassas alan yoktur; tüm kupon meta verisi içerilir.
/// Type geçerli değerleri: <c>Percent</c> | <c>Fixed</c>.
/// </summary>
public sealed record CouponResponse(
    Guid Id,
    string Code,
    string? Description,
    string Type,
    decimal Value,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int UsageCount,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    bool IsActive,
    Guid? RestrictedUserId,
    DateTime CreatedAt);
