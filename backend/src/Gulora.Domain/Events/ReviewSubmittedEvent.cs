using Gulora.Domain.Common;

namespace Gulora.Domain.Events;

/// <summary>
/// Kullanıcı ürün yorumu gönderdiğinde yayınlanır.
/// Handler'lar: moderasyon kuyruğuna ekleme, ürün ortalama puan güncelleme.
/// </summary>
public sealed record ReviewSubmittedEvent(
    Guid ReviewId,
    Guid ProductId,
    Guid UserId,
    int Rating
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
