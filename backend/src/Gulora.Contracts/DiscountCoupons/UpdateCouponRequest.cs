namespace Gulora.Contracts.DiscountCoupons;

/// <summary>
/// PUT api/v1/admin/coupons/{id} gövdesi (Admin).
/// Kupon kodu, türü ve değeri oluşturma sonrası değiştirilemez (iş kuralı);
/// yalnızca meta veri ve kısıtlamalar güncellenebilir.
/// Kuponu pasifleştirmek için <c>IsActive = false</c> gönderin.
/// </summary>
public sealed record UpdateCouponRequest(
    string? Description,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    bool IsActive);
