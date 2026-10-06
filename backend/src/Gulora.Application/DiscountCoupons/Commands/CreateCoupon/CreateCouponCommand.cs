using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Commands.CreateCoupon;

/// <summary>Yeni indirim kuponu oluşturur. Başarıda Guid döner.</summary>
public sealed record CreateCouponCommand(
    string Code,
    DiscountType Type,
    decimal Value,
    string? Description = null,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int? UsageLimit = null,
    DateTime? StartsAt = null,
    DateTime? ExpiresAt = null,
    Guid? RestrictedUserId = null
) : IRequest<Guid>;
