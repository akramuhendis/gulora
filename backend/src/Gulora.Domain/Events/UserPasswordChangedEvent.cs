using Gulora.Domain.Common;

namespace Gulora.Domain.Events;

/// <summary>
/// Kullanıcı parolası değiştirildiğinde yayınlanır.
/// Handler'lar: tüm refresh token'ları geçersiz kılma, güvenlik bildirimi e-postası.
/// </summary>
public sealed record UserPasswordChangedEvent(
    Guid UserId,
    string Email
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
