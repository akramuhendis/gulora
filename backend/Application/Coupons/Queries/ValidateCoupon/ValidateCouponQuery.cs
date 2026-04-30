using Gulora.Contracts.Coupons;
using MediatR;

namespace Gulora.Application.Coupons.Queries.ValidateCoupon;

/// <summary>
/// Sepet veya sipariş öncesi aşamada kupon kodunun geçerliliğini ve uygulanabilir indirim tutarını hesaplayan sorgu.
/// </summary>
/// <remarks>
/// Yanıt <see cref="ValidateCouponResponse"/> ile başarı durumu, kullanıcıya gösterilecek mesaj veya indirim
/// etiketi ve hesaplanan indirim tutarını taşır; kalıcı kullanım sayacı güncellemesi bu sorgunun kapsamı dışında olabilir.
/// </remarks>
/// <param name="Code">Kullanıcının girdiği kupon kodu; işleyicide kırpma ve büyük harfe normalize edilir.</param>
/// <param name="OrderAmount">
/// İndirimin hesaplanacağı referans tutar (ör. sepet ara toplamı); minimum sipariş ve yüzde indirim eşiği için kullanılır.
/// </param>
public sealed record ValidateCouponQuery(string Code, decimal OrderAmount) : IRequest<ValidateCouponResponse>;
