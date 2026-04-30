using Gulora.Domain.Events;

namespace Gulora.Domain.Common;

/// <summary>
/// Tüm kalıcı domain varlıkları için ortak özellikler.
/// Kimlik ve zaman damgaları dışarıdan doğrudan atanamaz; yalnızca türetilmiş sınıflar
/// <c>protected set</c> ve <see cref="SetUpdated"/> ile günceller.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Birincil anahtar; yeni örnek oluşturulduğunda otomatik <see cref="Guid"/> atanır.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>Kaydın ilk oluşturulma zamanı (UTC). EF ile veritabanına yazılırken de tutarlılık için UTC kullanılır.</summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>Son güncelleme zamanı; ilk kayıtta null, her anlamlı değişiklikte doldurulur.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Kuyruktaki olayların bir kopyasını döndürür ve kuyruğu boşaltır (genelde birim işi sonunda).</summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var copy = _domainEvents.ToArray();
        _domainEvents.Clear();
        return copy;
    }

    /// <summary>Türetilmiş sınıflar anlamlı yaşam döngüsü anlarında olay ekler; EF tarafından eşlenmez.</summary>
    protected void RegisterDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Varlık üzerinde domain kuralı ile yapılan değişikliklerden sonra çağrılır.</summary>
    protected void SetUpdated() => UpdatedAt = DateTime.UtcNow;
}