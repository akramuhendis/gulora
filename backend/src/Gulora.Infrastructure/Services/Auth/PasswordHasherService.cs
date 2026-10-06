using Gulora.Domain.Services;

namespace Gulora.Infrastructure.Services.Auth;

/// <summary>
/// BCrypt tabanlı parola hashing ve doğrulama servisi.
/// Parola düz metin olarak asla loglanmaz (gulora-security.mdc).
/// Work factor 12 — performans/güvenlik dengesi için proje standardı.
/// </summary>
internal sealed class PasswordHasherService : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainTextPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainTextPassword);
        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
    }

    public bool Verify(string plainTextPassword, string hash)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(hash))
            return false;

        return BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
    }
}
