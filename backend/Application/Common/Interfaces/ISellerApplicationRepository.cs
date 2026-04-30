using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Satıcı (seller) adayı başvuruları <see cref="SellerApplication"/>. Aynı e-posta için
/// beklemedeki başvuru varken ikinci kabul edilmez (use-case, bu sorgu ile pekişir).
/// </summary>
public interface ISellerApplicationRepository
{
    /// <summary>Verilen e-posta için hâlâ sonuçlanmamış (onay/ret) başvuru var mı.</summary>
    Task<bool> HasPendingApplicationAsync(string email, CancellationToken ct = default);

    /// <summary>Yeni satıcı başvurusu kaydı ekler (domain invariants sonrası).</summary>
    Task AddAsync(SellerApplication application, CancellationToken ct = default);

    /// <summary>Tüm toplu değişiklikleri veritabanına işler.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
