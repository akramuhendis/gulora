namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Kullanıcı parolasının (BCrypt/Argon/ASP.NET <c>PasswordHasher</c> sarmalayıcısı) güvenli
/// <i>hash</i>’lenmesi ve parola girdiği ile saklanan hash’in eşleşip eşleşmediğini doğrulamak.
/// Düz parola dışa verilmez, veritabanına düz metin gitmez.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Verilen düz <paramref name="password"/> için tek yönlü, tuzlu hash string üretir.</summary>
    string Hash(string password);

    /// <summary>Kullanıcı girişi ile kayıtlı <paramref name="passwordHash"/>'i güvenli şekilde karşılaştırır.</summary>
    bool Verify(string password, string passwordHash);
}
