namespace Gulora.Domain.Enums;

/// <summary>
/// Siparişin yaşam döngüsündeki durum. Değerler veritabanında sayı olarak saklanır;
/// sıra, iş kurallarındaki geçiş zinciriyle uyumlu olmalıdır.
/// </summary>
public enum OrderStatus
{
    /// <summary>Ödeme / onay bekleniyor.</summary>
    Pending = 0,
    /// <summary>Ödeme veya mağaza onayı alındı.</summary>
    Confirmed = 1,
    /// <summary>Paketleme veya hazırlık aşamasında.</summary>
    Preparing = 2,
    /// <summary>Kargoya veya kuryeye verildi.</summary>
    Shipped = 3,
    /// <summary>Müşteriye teslim edildi.</summary>
    Delivered = 4,
    /// <summary>Sipariş iptal edildi.</summary>
    Cancelled = 5,
    /// <summary>Müşteri iade talebinde bulundu.</summary>
    ReturnRequested = 6,
    /// <summary>İade işlemi tamamlandı.</summary>
    Returned = 7
}
