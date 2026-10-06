namespace Gulora.Contracts.DiscountCoupons;

/// <summary>
/// Kupon doğrulama yanıtı — GET api/v1/coupons/validate?code={code}&orderAmount={amount}.
/// Hesaplanan indirim tutarı ve mesaj içerir.
/// </summary>
public sealed record ValidateCouponResponse(
    bool IsValid,
    string Code,
    string Type,
    decimal Value,
    decimal DiscountAmount,
    decimal? MaxDiscountAmount,
    string? Message = null);
