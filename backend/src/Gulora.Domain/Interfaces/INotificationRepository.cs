using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Bildirim sorgu operasyonları.
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    /// <summary>Kullanıcının okunmamış bildirimlerini getirir.</summary>
    Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Kullanıcının tüm bildirimlerini sayfalı getirir.</summary>
    Task<IReadOnlyList<Notification>> GetByUserPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Kullanıcının okunmamış bildirim sayısını döner.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Kullanıcının toplam bildirim sayısını döner (sayfalama için).</summary>
    Task<int> GetTotalCountAsync(Guid userId, CancellationToken ct = default);
}
