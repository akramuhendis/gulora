using Gulora.Domain.Common;
using System.Linq.Expressions;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Generic repository arayüzü — tüm aggregate root'lar için temel CRUD operasyonları.
/// Uygulama Infrastructure katmanındadır; Domain/Application sadece bu arayüzü kullanır.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Kimlikle tekil kayıt getirir; bulunamazsa null döner.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Filtre koşuluna göre tekil kayıt getirir.</summary>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Filtre koşuluna göre kayıt listesi getirir.</summary>
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>Sayfalı kayıt listesi getirir.</summary>
    Task<IReadOnlyList<T>> ListPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Koşula uyan kayıt sayısını döner.</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>Verilen koşulu karşılayan en az bir kayıt var mı?</summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Yeni varlık ekler (henüz persist edilmez; UoW ile birlikte commit edilir).</summary>
    void Add(T entity);

    /// <summary>Birden fazla yeni varlık ekler.</summary>
    void AddRange(IEnumerable<T> entities);

    /// <summary>Varlığı günceller (tracking üzerinden; explicit çağrı genellikle gerekmez).</summary>
    void Update(T entity);

    /// <summary>Varlığı siler (aggregate root için fiziksel silme — soft delete entity metodunda).</summary>
    void Remove(T entity);
}
