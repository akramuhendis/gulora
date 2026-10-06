namespace Gulora.Domain.Common;

/// <summary>
/// Tüm Domain varlıklarının temel sınıfı.
/// Kimlik, zaman damgaları ve domain olay toplama mekanizmasını barındırır.
/// Dış bağımlılık yoktur — EF Core attribute'ları bu sınıfta kullanılmaz.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Bu varlık üzerinde toplanan domain olaylarının salt-okunur listesi.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Infrastructure (SaveChanges interceptor veya Unit of Work) tarafından,
    /// olay yayınlandıktan sonra listeyi temizlemek için çağrılır.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Kalıcı güncelleme zaman damgasını ayarlar.
    /// Entity yöntemi aracılığıyla değişiklik yapıldığında çağrılmalıdır.
    /// </summary>
    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}
