using Gulora.Domain.Common;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Events;

/// <summary>
/// Yeni bir sipariş başarıyla oluşturulduğunda yayınlanır.
/// Handler'lar: bildirim gönderme, stok rezervasyonu, kargo hazırlığı.
/// </summary>
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    string OrderNumber,
    decimal TotalAmount,
    PaymentMethod PaymentMethod,
    DeliveryType DeliveryType
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
