using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;

namespace Gulora.Infrastructure.Services.Domain;

/// <summary>
/// İndirim kuponu hesaplama servisi.
/// İş kuralı Domain katmanında (DiscountCoupon.ValidateFor), hesaplama burada.
/// </summary>
internal sealed class DiscountCalculatorService : IDiscountCalculator
{
    private readonly IDiscountCouponRepository _couponRepository;

    public DiscountCalculatorService(IDiscountCouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<Money> CalculateDiscountAsync(
        DiscountCoupon coupon,
        Money orderTotal,
        Guid userId,
        CancellationToken ct = default)
    {
        // Domain iş kuralı doğrulaması (geçersizse exception fırlatır)
        coupon.ValidateFor(userId, orderTotal.Amount, DateTime.UtcNow);

        // Kullanıcı bu kuponu daha önce kullandı mı?
        var alreadyUsed = await _couponRepository.HasUserUsedCouponAsync(coupon.Id, userId, ct);
        if (alreadyUsed)
            throw new Gulora.Domain.Exceptions.BusinessRuleException(
                "Bu kuponu daha önce kullandınız.",
                "COUPON_ALREADY_USED");

        var discountAmount = coupon.Type switch
        {
            DiscountType.Percent => orderTotal.ApplyPercent(coupon.Value),
            DiscountType.Fixed => Money.Of(Math.Min(coupon.Value, orderTotal.Amount)),
            _ => Money.Zero
        };

        // MaxDiscountAmount kısıtlaması
        if (coupon.MaxDiscountAmount.HasValue && discountAmount.Amount > coupon.MaxDiscountAmount.Value)
            discountAmount = Money.Of(coupon.MaxDiscountAmount.Value);

        return discountAmount;
    }
}
