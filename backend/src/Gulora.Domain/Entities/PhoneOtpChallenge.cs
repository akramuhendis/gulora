using Gulora.Domain.Common;

namespace Gulora.Domain.Entities;

/// <summary>
/// Telefon OTP doğrulaması — kod hash ile saklanır, düz metin kalıcı yazılmaz.
/// </summary>
public sealed class PhoneOtpChallenge : BaseEntity
{
    public string PhoneNumber { get; private set; } = string.Empty;
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime SentAt { get; private set; }

    private PhoneOtpChallenge() { }

    public static PhoneOtpChallenge Create(string normalizedPhone, string codeHash, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedPhone);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        var now = DateTime.UtcNow;
        return new PhoneOtpChallenge
        {
            PhoneNumber = normalizedPhone.Trim(),
            CodeHash = codeHash,
            SentAt = now,
            ExpiresAt = now.Add(lifetime)
        };
    }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;
}
