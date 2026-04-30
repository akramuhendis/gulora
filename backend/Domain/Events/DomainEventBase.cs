namespace Gulora.Domain.Events;

/// <summary>
/// Somut domain olayları için temel kayıt tipi. <see cref="OccurredOn"/> varsayılan olarak
/// örnek oluşturulduğu anı kullanır; gerekirse türetilen kayıtta açıkça atanabilir.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    /// <inheritdoc />
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
