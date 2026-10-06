namespace Gulora.Domain.Interfaces;

/// <summary>
/// Unit of Work deseni — tek bir işlem kapsamında birden fazla repository değişikliğini
/// atomik olarak commit eder ve domain olaylarını yayınlar.
/// Uygulama Infrastructure'da (EF Core DbContext üzerinden) gerçekleştirilir.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Tüm bekleyen değişiklikleri veritabanına yazar ve domain olaylarını dağıtır.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Explicit transaction başlatır (çok-adımlı atomik işlem için).</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Açık transaction'ı commit eder.</summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>Açık transaction'ı geri alır.</summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
