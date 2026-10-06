namespace Gulora.Domain.Enums;

/// <summary>Ödeme işleminin durumu.</summary>
public enum PaymentStatus
{
    /// <summary>Ödeme bekleniyor.</summary>
    Pending = 1,

    /// <summary>Ödeme başarıyla tamamlandı.</summary>
    Completed = 2,

    /// <summary>Ödeme başarısız oldu.</summary>
    Failed = 3,

    /// <summary>Ödeme iade edildi.</summary>
    Refunded = 4,

    /// <summary>Kısmi iade yapıldı.</summary>
    PartiallyRefunded = 5
}
