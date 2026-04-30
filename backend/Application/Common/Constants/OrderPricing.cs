namespace Gulora.Application.Common.Constants;

/// <summary>
/// Sipariş oluşturma ve fiyatlandırma ile ilgili, uygulama genelinde kullanılan sabit değerler.
/// İhtiyaç halinde <c>appsettings</c> veya ayrı bir fiyat servisine taşınabilir.
/// </summary>
public static class OrderPricing
{
    /// <summary>Standart kargo/teslimat ücreti (manat cinsinden, iş kuralı varsayılanı).</summary>
    public const decimal StandardDeliveryFeeAzn = 5m;
}
