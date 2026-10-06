using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// User aggregate'ine özgü sorgu operasyonları.
/// Genel CRUD için <see cref="IRepository{User}"/> miras alınır.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>E-posta adresine göre kullanıcı getirir (büyük/küçük harf duyarsız).</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>E-posta adresinin sistemde kayıtlı olup olmadığını kontrol eder.</summary>
    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);

    /// <summary>Aktif refresh token'a sahip kullanıcıyı getirir.</summary>
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);

    /// <summary>E-posta doğrulama jetonuna göre kullanıcı getirir.</summary>
    Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken ct = default);
}
