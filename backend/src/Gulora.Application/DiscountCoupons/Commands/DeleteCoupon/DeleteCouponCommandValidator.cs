using FluentValidation;

namespace Gulora.Application.DiscountCoupons.Commands.DeleteCoupon;

public sealed class DeleteCouponCommandValidator : AbstractValidator<DeleteCouponCommand>
{
    public DeleteCouponCommandValidator()
    {
        RuleFor(x => x.CouponId)
            .NotEmpty().WithMessage("Kupon kimliği boş olamaz.");
    }
}
