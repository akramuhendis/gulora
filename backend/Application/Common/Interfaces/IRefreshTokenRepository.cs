using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// <see cref="RefreshToken"/> (yenileme jetonu) saklama, düz metin/ hash ile sorgu ve
/// tüm cihazlarda oturum kapatma (tüm belirteçlerin geçersiz bırakılması) gibi operasyonlar.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>İstemcinin gönderdiği yenileme token değerine denk gelen kayıt; iptal/rotasyon için.</summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Login/Register sonrası yeni dönen düz <paramref name="token"/> satırını yazar.</summary>
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Belirli kullanıcıya ait tüm belirteçleri iptal eder (güvenlik: şifre değişikliği vb.).</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Rotasyon, iptal bayrakları vb. kalıcı olsun diye toplu kayıt.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
