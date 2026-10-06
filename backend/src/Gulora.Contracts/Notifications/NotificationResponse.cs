namespace Gulora.Contracts.Notifications;

/// <summary>
/// Kullanıcı bildirimi yanıtı — GET api/v1/notifications.
/// Type: OrderConfirmed | OrderShipped | OrderDelivered | OrderCancelled |
///       ReturnApproved | PasswordChanged | PromotionAlert.
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTime? ReadAt,
    Guid? RelatedEntityId,
    DateTime CreatedAt);
