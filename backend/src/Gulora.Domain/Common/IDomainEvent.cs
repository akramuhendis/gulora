namespace Gulora.Domain.Common;

/// <summary>
/// Domain katmanında yayınlanan tüm olayların temel işaretleyici arayüzü.
/// MediatR INotification ile eşleştirilmesi Application katmanında yapılır.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Olayın oluşturulduğu UTC zaman damgası.</summary>
    DateTime OccurredOn { get; }
}
