using Gulora.Domain.Common;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcıya ait refresh token kaydı.
/// User aggregate'inin alt entity'sidir — doğrudan repository üzerinden erişilmez.
/// Token değeri loglanmaz (güvenlik kuralı).
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }

    /// <summary>Token aktif ve süresi dolmamış mı?</summary>
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    /// <summary>Token iptal edildi mi?</summary>
    public bool IsRevoked => RevokedAt is not null;

    /// <summary>Token süresi doldu mu?</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Token bitiş tarihi geçmişte olamaz.", nameof(expiresAt));

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    /// <summary>Token'ı iptal eder ve yerine geçen token'ı kaydeder.</summary>
    public void Revoke(string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
