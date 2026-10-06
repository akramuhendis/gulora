using MediatR;

namespace Gulora.Application.DiscountCoupons.Commands.UpdateCoupon;

public sealed record UpdateCouponCommand(
    Guid CouponId,
    string? Description,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    bool IsActive) : IRequest;
