using Gulora.Domain.Common;

namespace Gulora.Domain.Entities;

/// <summary>
/// JWT erişim belirtecinin süresi dolduğunda yeni erişim belirteci almak için kullanılan
/// yenileme (refresh) belirtecini temsil eder. Güvenlik için veritabanında saklanır ve
/// iptal edilebilir (revoke).
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>Bu belirtecin ait olduğu kullanıcı.</summary>
    public Guid UserId { get; private set; }
    /// <summary>Tek kullanımlık veya uzun ömürlü rastgele token dizesi.</summary>
    public string Token { get; private set; } = default!;
    /// <summary>UTC zamanında son geçerlilik anı; sonrasında yenileme reddedilir.</summary>
    public DateTime ExpiresAt { get; private set; }
    /// <summary>Çıkış yapıldığında veya şüpheli aktivitede true yapılır.</summary>
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    /// <summary>Yeni bir yenileme belirteci üretir.</summary>
    /// <param name="userId">Sahip kullanıcı kimliği.</param>
    /// <param name="token">Kriptografik olarak güvenli üretilmiş dize.</param>
    /// <param name="lifetimeDays">Geçerlilik süresi (gün).</param>
    public static RefreshToken Create(Guid userId, string token, int lifetimeDays)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lifetimeDays);
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            // Tüm tarihler UTC ile tutulur; sunucu saat diliminden bağımsızlık
            ExpiresAt = DateTime.UtcNow.AddDays(lifetimeDays)
        };
    }

    /// <summary>Süre dolduysa true.</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    /// <summary>Hem iptal edilmemiş hem de süresi dolmamışsa true.</summary>
    public bool IsValid => !IsRevoked && !IsExpired;

    /// <summary>Belirteci geçersiz kılar (ör. çıkış veya tüm oturumları sonlandırma); təkrar çağrı idempotentdir.</summary>
    public void Revoke()
    {
        if (IsRevoked) return;
        IsRevoked = true;
        SetUpdated();
    }
}
