using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Kullanıcı teslimat <see cref="Address"/> varlıklarının veri erişim sözleşmesi. Kalıcılık (EF Core vb.)
/// <c>Gulora.Infrastructure</c> katmanında uygulanır; uygulama katmanı sadece bu arayüze bağlanır.
/// </summary>
public interface IAddressRepository
{
    /// <summary>Belirtilen kullanıcının tüm kayıtlı adreslerini döner (sıra: iş kuralına/uygulamaya bağlı).</summary>
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Öncelik: güncelleme-silme için adrese kimlik ile erişim.</summary>
    Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Yeni bir adresi bağlama ekler (henüz kalıcı olmayabilir; <see cref="SaveChangesAsync"/> gerekir).</summary>
    Task AddAsync(Address address, CancellationToken ct = default);

    /// <summary>Var olan adresi bağlamdan siler (soft delete değilse tablodan kaldırır).</summary>
    Task RemoveAsync(Address address, CancellationToken ct = default);

    /// <summary>Ünite içinde biriken tüm değişiklikleri veritabanına yansıtır.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
