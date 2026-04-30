using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Favori ürün (kullanıcı-ürün çifti) ilişkileri. Listeleme, var mı sorgu, ekleme, silme ve
/// toplu kalıcılık için kullanılır.
/// </summary>
public interface IFavoriteRepository
{
    /// <summary>Kullanıcının tüm favori <see cref="Favorite"/> kayıtlarını (ürünle birlikte yüklenebilir) döner.</summary>
    Task<IReadOnlyList<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Belirli (kullanıcı, ürün) çifti için tekil satır; silme/çift tık önleme.</summary>
    Task<Favorite?> GetAsync(Guid userId, Guid productId, CancellationToken ct = default);

    /// <summary>Domain’den oluşmuş <see cref="Favorite"/> ekleme.</summary>
    Task AddAsync(Favorite favorite, CancellationToken ct = default);

    /// <summary>Var olanı kaldırma; kayıt yoksa use-case fırlatabilir (handler sorumluluğunda).</summary>
    Task RemoveAsync(Favorite favorite, CancellationToken ct = default);

    /// <summary>Değişiklikleri veritabanına işler.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
