namespace Gulora.Contracts.DiscountCoupons;

/// <summary>
/// POST api/v1/admin/coupons gövdesi (Admin).
/// Type geçerli değerleri: <c>Percent</c> (yüzde indirim) | <c>Fixed</c> (sabit tutar indirimi).
/// Percent türünde Value 1–100 arasında olmalıdır.
/// </summary>
public sealed record CreateCouponRequest(
    string Code,
    string Type,
    decimal Value,
    string? Description = null,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int? UsageLimit = null,
    DateTime? StartsAt = null,
    DateTime? ExpiresAt = null,
    Guid? RestrictedUserId = null);
