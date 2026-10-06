using Gulora.Contracts.DiscountCoupons;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;
using MediatR;

namespace Gulora.Application.DiscountCoupons.Queries.ValidateCoupon;

public sealed class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResponse>
{
    private readonly IDiscountCouponRepository _couponRepository;
    private readonly IDiscountCalculator _discountCalculator;

    public ValidateCouponQueryHandler(
        IDiscountCouponRepository couponRepository,
        IDiscountCalculator discountCalculator)
    {
        _couponRepository = couponRepository;
        _discountCalculator = discountCalculator;
    }

    public async Task<ValidateCouponResponse> Handle(ValidateCouponQuery query, CancellationToken ct)
    {
        var coupon = await _couponRepository.GetByCodeAsync(query.Code, ct);
        if (coupon is null || !coupon.IsValidAt(DateTime.UtcNow))
        {
            return new ValidateCouponResponse(
                false, query.Code, string.Empty, 0, 0, null,
                "Kupon geçersiz veya süresi dolmuş.");
        }

        try
        {
            coupon.ValidateFor(query.UserId, query.OrderTotal, DateTime.UtcNow);
            var discount = await _discountCalculator.CalculateDiscountAsync(
                coupon,
                Money.Of(query.OrderTotal),
                query.UserId,
                ct);

            return new ValidateCouponResponse(
                true,
                coupon.Code,
                coupon.Type.ToString(),
                coupon.Value,
                discount.Amount,
                coupon.MaxDiscountAmount);
        }
        catch (Exception ex)
        {
            return new ValidateCouponResponse(
                false, coupon.Code, coupon.Type.ToString(), coupon.Value, 0, null, ex.Message);
        }
    }
}
