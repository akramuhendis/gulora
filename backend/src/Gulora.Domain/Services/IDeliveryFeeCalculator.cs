using Gulora.Domain.Enums;
using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Services;

/// <summary>
/// Teslimat ücretini hesaplayan domain servis arayüzü.
/// Kural: belirli eşiğin üzerindeki siparişlerde ücretsiz teslimat uygulanabilir.
/// </summary>
public interface IDeliveryFeeCalculator
{
    /// <summary>
    /// Teslimat türü ve sipariş tutarına göre teslimat ücretini hesaplar.
    /// Ücretsiz teslimat eşiği aşılmışsa <see cref="Money.Zero"/> döner.
    /// </summary>
    Money Calculate(DeliveryType deliveryType, Money orderSubtotal);
}
