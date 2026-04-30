namespace Gulora.Contracts.Coupons;

/// <summary>User-facing Azerbaijani strings for coupon validation API responses.</summary>
public static class ValidateCouponUserMessages
{
    public const string NotFound = "Kupon tapılmadı.";
    public const string NotValidForOrder = "Kupon bu sifariş üçün etibarlı deyil.";

    public const string PercentDiscountFormat = "{0}% endirim";
    public const string FixedAmountDiscountFormat = "{0:F2} AZN endirim";
}
