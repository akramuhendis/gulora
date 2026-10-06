using Gulora.Domain.Enums;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace Gulora.Infrastructure.Services.Domain;

/// <summary>
/// Teslimat ücreti hesaplama servisi.
/// Eşik değerler yapılandırma üzerinden okunur (appsettings).
/// Kural: sipariş toplamı eşiği geçerse ücretsiz teslimat (Standard için).
/// </summary>
internal sealed class DeliveryFeeCalculatorService : IDeliveryFeeCalculator
{
    private readonly IConfiguration _configuration;

    public DeliveryFeeCalculatorService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Money Calculate(DeliveryType deliveryType, Money orderSubtotal)
    {
        var freeDeliveryThreshold = _configuration.GetValue("Delivery:FreeThreshold", 50m);
        var standardFee = _configuration.GetValue("Delivery:StandardFee", 5m);
        var expressFee = _configuration.GetValue("Delivery:ExpressFee", 12m);
        var sameDayFee = _configuration.GetValue("Delivery:SameDayFee", 20m);

        return deliveryType switch
        {
            DeliveryType.Standard when orderSubtotal.Amount >= freeDeliveryThreshold
                => Money.Zero,
            DeliveryType.Standard
                => Money.Of(standardFee),
            DeliveryType.Express
                => Money.Of(expressFee),
            DeliveryType.SameDay
                => Money.Of(sameDayFee),
            _ => Money.Of(standardFee)
        };
    }
}
