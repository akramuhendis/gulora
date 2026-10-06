namespace Gulora.Domain.Enums;

/// <summary>Teslimat hız/tier seçenekleri.</summary>
public enum DeliveryType
{
    /// <summary>Standart teslimat (3-5 iş günü).</summary>
    Standard = 1,

    /// <summary>Hızlı teslimat (1-2 iş günü).</summary>
    Express = 2,

    /// <summary>Aynı gün teslimat.</summary>
    SameDay = 3
}
