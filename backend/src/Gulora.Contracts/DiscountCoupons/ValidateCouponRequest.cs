namespace Gulora.Contracts.DiscountCoupons;

/// <summary>
/// GET api/v1/coupons/validate sorgu parametreleri.
/// Kullanıcı ödeme öncesinde kuponun geçerliliğini ve indirim tutarını sorgular.
/// Örnek: GET api/v1/coupons/validate?code=YENI10&amp;orderSubTotal=250.00
/// </summary>
public sealed record ValidateCouponRequest
{
    /// <summary>Doğrulanacak kupon kodu.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>İndirim hesabına baz alınacak sipariş ara toplamı (kargo ve indirim öncesi).</summary>
    public decimal OrderSubTotal { get; init; }
}
