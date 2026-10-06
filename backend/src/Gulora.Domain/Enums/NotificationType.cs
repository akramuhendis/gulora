namespace Gulora.Domain.Enums;

/// <summary>Sistem bildirimi türleri.</summary>
public enum NotificationType
{
    OrderConfirmed = 1,
    OrderShipped = 2,
    OrderDelivered = 3,
    OrderCancelled = 4,
    ReturnApproved = 5,
    PasswordChanged = 6,
    PromotionAlert = 7
}
