using Gulora.Domain.Common;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Events;

/// <summary>
/// Yeni kullanıcı kaydı tamamlandığında yayınlanır.
/// Handler'lar: hoş geldin e-postası, segmentasyon, analitik.
/// </summary>
public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FullName,
    UserRole Role
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
