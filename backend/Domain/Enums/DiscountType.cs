namespace Gulora.Domain.Enums;

/// <summary>
/// Kupon veya kampanya indiriminin hesaplanma biçimi.
/// </summary>
public enum DiscountType
{
    /// <summary>Sepet tutarı üzerinden yüzde; 0–100 üst sınırı <see cref="Gulora.Domain.Entities.Coupon"/> oluşturma kurallarında denetlenir.</summary>
    Percent = 0,
    /// <summary>Sabit para birimi tutarı; sepet tutarını aşmayacak şekilde kısıtlanır.</summary>
    Fixed = 1
}
