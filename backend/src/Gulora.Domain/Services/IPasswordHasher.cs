namespace Gulora.Domain.Services;

/// <summary>
/// Parola karma ve doğrulama domain servis arayüzü.
/// BCrypt veya başka bir tek yönlü karma algoritması kullanılır;
/// uygulama Infrastructure katmanındadır.
/// Parola asla loglanmaz veya açık metin olarak saklanmaz.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Düz metin parolayı hashler ve sonucu döner.</summary>
    string Hash(string plainTextPassword);

    /// <summary>Düz metin parola ile hash'i doğrular.</summary>
    bool Verify(string plainTextPassword, string hash);
}
