using Gulora.Domain.Common;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Entities;

/// <summary>
/// Siparişin durum geçişlerinin denetim izi.
/// Order aggregate'inin alt entity'sidir; silinmez.
/// Her durum değişikliğinde yeni bir kayıt eklenir.
/// </summary>
public sealed class OrderTracking : BaseEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }
    public string? CargoCompany { get; private set; }
    public string? TrackingNumber { get; private set; }
    public Guid? ChangedByUserId { get; private set; }

    private OrderTracking() { }

    public static OrderTracking Create(
        Guid orderId,
        OrderStatus status,
        string? note = null,
        string? cargoCompany = null,
        string? trackingNumber = null,
        Guid? changedByUserId = null)
    {
        return new OrderTracking
        {
            OrderId = orderId,
            Status = status,
            Note = note?.Trim(),
            CargoCompany = cargoCompany?.Trim(),
            TrackingNumber = trackingNumber?.Trim(),
            ChangedByUserId = changedByUserId
        };
    }
}
