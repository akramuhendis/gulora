namespace Gulora.Domain.Enums;

/// <summary>
/// Siparişin yaşam döngüsü boyunca geçebileceği durum kümesi.
/// Durum geçiş kuralları <see cref="Gulora.Domain.Entities.Order"/> aggregate root içinde korunur.
/// </summary>
public enum OrderStatus
{
    /// <summary>Sipariş alındı, henüz onaylanmadı.</summary>
    Pending = 1,

    /// <summary>Satıcı/sistem tarafından onaylandı.</summary>
    Confirmed = 2,

    /// <summary>Hazırlanıyor / paketleniyor.</summary>
    Processing = 3,

    /// <summary>Kargo firmasına teslim edildi.</summary>
    Shipped = 4,

    /// <summary>Müşteriye teslim edildi.</summary>
    Delivered = 5,

    /// <summary>Sipariş iptal edildi.</summary>
    Cancelled = 6,

    /// <summary>İade talebi açıldı.</summary>
    ReturnRequested = 7,

    /// <summary>İade tamamlandı.</summary>
    Returned = 8
}
