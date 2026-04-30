namespace Gulora.Contracts.Coupons;

public record ValidateCouponResponse(
    bool IsValid,
    string? ErrorMessage,
    decimal DiscountAmount,
    string DiscountLabel);
