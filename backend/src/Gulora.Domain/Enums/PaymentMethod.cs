namespace Gulora.Domain.Enums;

/// <summary>Desteklenen ödeme yöntemleri.</summary>
public enum PaymentMethod
{
    /// <summary>Kredi kartı ile online ödeme.</summary>
    CreditCard = 1,

    /// <summary>Banka/debit kartı ile online ödeme.</summary>
    DebitCard = 2,

    /// <summary>Kapıda nakit ödeme.</summary>
    CashOnDelivery = 3,

    /// <summary>Banka havalesi / EFT.</summary>
    BankTransfer = 4
}
