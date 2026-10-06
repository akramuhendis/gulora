using Gulora.Contracts.Common;

namespace Gulora.Contracts.Notifications;

/// <summary>GET api/v1/notifications sorgu parametreleri.</summary>
public sealed record GetNotificationsRequest : PagedRequest
{
    /// <summary>Yalnızca okunmamış bildirimleri getir.</summary>
    public bool? UnreadOnly { get; init; }
}
