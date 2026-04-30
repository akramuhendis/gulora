using FluentValidation;

namespace Gulora.Application.Coupons.Queries.ValidateCoupon;

/// <summary>
/// <see cref="ValidateCouponQuery"/> için kupon kodu ve sipariş tutarı alanlarının temel doğrulaması.
/// </summary>
/// <remarks>
/// Gerçek kupon varlığı ve tarih/limit kuralları işleyicide; burada yalnızca istek biçimi kontrol edilir.
/// </remarks>
public sealed class ValidateCouponQueryValidator : AbstractValidator<ValidateCouponQuery>
{
    /// <summary>
    /// Kodun boş olmaması, uzunluk sınırı ve sipariş tutarının negatif olmaması kurallarını ekler.
    /// </summary>
    public ValidateCouponQueryValidator()
    {
        // Anlamsız veya aşırı uzun kod ile depo yükünü sınırla
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        // Negatif tutar ile indirim hesabı anlamsız
        RuleFor(x => x.OrderAmount).GreaterThanOrEqualTo(0);
    }
}
