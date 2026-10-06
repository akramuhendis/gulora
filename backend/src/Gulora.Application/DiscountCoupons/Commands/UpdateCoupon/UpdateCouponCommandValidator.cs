using FluentValidation;

namespace Gulora.Application.DiscountCoupons.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.CouponId).NotEmpty();
        RuleFor(x => x.UsageLimit).GreaterThanOrEqualTo(0).When(x => x.UsageLimit.HasValue);
    }
}
