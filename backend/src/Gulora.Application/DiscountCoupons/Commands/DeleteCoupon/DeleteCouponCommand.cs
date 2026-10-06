using MediatR;

namespace Gulora.Application.DiscountCoupons.Commands.DeleteCoupon;

/// <summary>Kuponu devre dışı bırakır (soft delete).</summary>
public sealed record DeleteCouponCommand(Guid CouponId) : IRequest;
