namespace Gulora.Domain.Enums;

/// <summary>
/// Teslimat hızı veya planlama modeli; kargo ücreti ve operasyon kuralları buna bağlanabilir.
/// </summary>
public enum DeliveryType
{
    /// <summary>Standart teslimat penceresi.</summary>
    Standard = 0,
    /// <summary>Aynı gün içinde teslimat (ürün ve bölge uygunluğu ayrıca kontrol edilir).</summary>
    SameDay = 1,
    /// <summary>Müşterinin seçtiği tarih/saat aralığına planlı teslimat.</summary>
    Scheduled = 2
}
