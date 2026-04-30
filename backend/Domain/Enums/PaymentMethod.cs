namespace Gulora.Domain.Enums;

/// <summary>
/// Sipariş için seçilen ödeme kanalı. Tahsilat ve muhasebe akışları bu alana göre dallanır.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Ön ödemeli kart veya cüzdan.</summary>
    Card = 0,
    /// <summary>Teslimatta nakit veya POS.</summary>
    CashOnDelivery = 1,
    /// <summary>Banka havalesi / EFT; onay sonrası işleme alınabilir.</summary>
    BankTransfer = 2
}
