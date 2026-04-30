namespace Gulora.Contracts.Coupons;

public record ValidateCouponRequest(string Code, decimal OrderAmount);
