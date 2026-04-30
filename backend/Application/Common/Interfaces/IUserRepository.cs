using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Sistem <see cref="User"/> (müşteri/yönetici) kayıtları. Kayıt, profil, oturum yenileme ve
/// inceleme listesinde toplu kullanıcı adı çekme gibi uçlar bu arayüz üzerindedir.
/// </summary>
public interface IUserRepository
{
    /// <summary>Kimliğe göre tek kullanıcı; yoksa <c>null</c> (GetMe, profil güncelleme).</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>E-posta ile oturum açma ve e-posta benzersizliği sorguları (normalize edilmiş e-posta ile çağrılmalıdır).</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Birden çok kullanıcıyı toplu getirir; ör. inceleme listesinde yazar isimlerini doldurma.</summary>
    Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>Kayıt anında: e-posta çakışması mı? (Benzersiz indeksle uyumlu sorgu.)</summary>
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);

    /// <summary>Kayıt sonrası yeni <see cref="User"/> ekleme.</summary>
    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>Parola, profil, rol gibi alanlardaki değişiklikleri kalıcı hale getirir.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
