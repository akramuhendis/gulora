using Gulora.Domain.Common;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Entities;

/// <summary>
/// Sipariş yaşam döngüsündeki tek bir olayı temsil eder (durum + kullanıcıya gösterilen mesaj + zaman).
/// Müşteri “sipariş nerede?” ekranında bu kayıtlar kronolojik sırayla listelenir.
/// </summary>
public class OrderTracking : BaseEntity
{
    /// <summary>İlgili sipariş.</summary>
    public Guid OrderId { get; private set; }
    /// <summary>O anda geçerli olan veya kaydedilen durum kodu.</summary>
    public OrderStatus Status { get; private set; }
    /// <summary>Müşteri dilinde kısa açıklama (Azerbaycan Türkçesi metinler API’den gelebilir).</summary>
    public string Message { get; private set; } = default!;
    /// <summary>Olayın oluştuğu an (UTC).</summary>
    public DateTime Timestamp { get; private set; }

    private OrderTracking() { }

    /// <summary>Yeni bir izleme kaydı oluşturur.</summary>
    public static OrderTracking Create(Guid orderId, OrderStatus status, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new OrderTracking
        {
            OrderId = orderId,
            Status = status,
            Message = message.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}
