using Gulora.Domain.Events;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Toplanan etki alanı (domain) olaylarını, kalıcılık katmanı başarılı olduktan sonra asenkron olarak
/// ileri sürmek (ör. loglama, entegrasyon, yan süreçler) için uygulama sözleşmesi.
/// Gerçek yürütme altyapı (ör. <c>ISaveChangesInterceptor</c> ile) Infrastructure'da uygulanır.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>Tek seferde bir veya daha fazla <see cref="IDomainEvent"/> yönlendirir.</summary>
    /// <param name="events">İşlenecek domain olayları listesi.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default);
}
