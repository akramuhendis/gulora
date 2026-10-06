using Gulora.Domain.Common;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcıya gönderilen sistem bildirimi kaydı.
/// E-posta gönderimi Infrastructure'da (MailKit) gerçekleşir;
/// bu entity bildirim durumunu takip eder.
/// </summary>
public sealed class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        Guid? relatedEntityId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new Notification
        {
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Body = body.Trim(),
            IsRead = false,
            RelatedEntityId = relatedEntityId
        };
    }

    /// <summary>Bildirimi okundu olarak işaretler.</summary>
    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        MarkUpdated();
    }
}
