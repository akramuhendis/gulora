namespace Gulora.Contracts.Notifications;

/// <summary>
/// PATCH api/v1/notifications/read gövdesi — toplu okundu işareti.
/// NotificationIds boş veya null ise kullanıcının tüm okunmamış bildirimleri okundu yapılır.
/// Belirli ID'ler verilirse yalnızca o bildirimler işaretlenir.
/// </summary>
public sealed record MarkNotificationsReadRequest
{
    /// <summary>
    /// Okundu işaretlenecek bildirim ID listesi.
    /// Boş bırakılırsa tüm okunmamış bildirimler toplu işaretlenir.
    /// </summary>
    public IReadOnlyList<Guid> NotificationIds { get; init; } = [];
}
