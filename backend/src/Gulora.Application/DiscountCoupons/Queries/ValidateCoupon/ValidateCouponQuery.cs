using Gulora.Contracts.DiscountCoupons;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Queries.ValidateCoupon;

/// <summary>
/// Sipariş ödeme öncesinde kuponu doğrular ve indirim tutarını önizler.
/// Bu işlem kuponu kullanmaz; sipariş oluşturma sırasında handler gerçek uygulamayı yapar.
/// </summary>
public sealed record ValidateCouponQuery(
    string Code,
    Guid UserId,
    decimal OrderTotal
) : IRequest<ValidateCouponResponse>;
