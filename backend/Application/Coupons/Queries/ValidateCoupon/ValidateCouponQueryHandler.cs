using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Coupons;
using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Coupons.Queries.ValidateCoupon;

/// <summary>
/// Kupon kodunu depoda arar; varlık üzerindeki iş kurallarına göre geçerlilik ve indirim miktarını <see cref="ValidateCouponResponse"/> olarak döndürür.
/// </summary>
/// <remarks>
/// Kod karşılaştırması için <see cref="string.Trim"/> ve <see cref="string.ToUpperInvariant"/> kullanılır;
/// indirim türüne göre kullanıcıya gösterilecek etiket metni yerelleştirilmiş şablonlarla üretilir.
/// </remarks>
public sealed class ValidateCouponQueryHandler(
    ICouponRepository coupons) : IRequestHandler<ValidateCouponQuery, ValidateCouponResponse>
{
    /// <summary>
    /// Kuponu yükler; yoksa veya geçersizse başarısız yanıt; geçerliyse indirim tutarı ve açıklama etiketi ile başarılı yanıt döner.
    /// </summary>
    /// <param name="request">Kupon kodu ve sipariş tutarı.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Doğrulama sonucu ve isteğe bağlı indirim bilgisi.</returns>
    public async Task<ValidateCouponResponse> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        // Depo ve karşılaştırmalar için tutarlı anahtar: boşluk kırpma ve büyük harf
        var code = request.Code.Trim().ToUpperInvariant();
        var coupon = await coupons.GetByCodeAsync(code, cancellationToken).ConfigureAwait(false);

        if (coupon is null)
            return new ValidateCouponResponse(
                false, ValidateCouponUserMessages.NotFound, 0, string.Empty);

        // Minimum tutar, tarih veya kullanım limiti gibi kurallar varlık içinde kontrol edilir
        if (!coupon.IsValid(request.OrderAmount))
            return new ValidateCouponResponse(
                false, ValidateCouponUserMessages.NotValidForOrder, 0, string.Empty);

        var discount = coupon.CalculateDiscount(request.OrderAmount);
        // Yüzde veya sabit tutar indirimine göre kullanıcıya gösterilecek etiket metni
        var label = coupon.DiscountType == DiscountType.Percent
            ? string.Format(ValidateCouponUserMessages.PercentDiscountFormat, coupon.DiscountValue)
            : string.Format(ValidateCouponUserMessages.FixedAmountDiscountFormat, coupon.DiscountValue);

        return new ValidateCouponResponse(true, null, discount, label);
    }
}
