using FluentValidation;

namespace Gulora.Application.DiscountCoupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kupon kodu boş olamaz.")
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Kupon kodu yalnızca büyük harf, rakam, tire ve alt çizgi içerebilir.");

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("İndirim değeri sıfırdan büyük olmalıdır.");

        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100).WithMessage("Yüzde indirim 100'den fazla olamaz.")
            .When(x => x.Type == Domain.Enums.DiscountType.Percent);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.StartsAt).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.")
            .When(x => x.ExpiresAt.HasValue && x.StartsAt.HasValue);
    }
}
