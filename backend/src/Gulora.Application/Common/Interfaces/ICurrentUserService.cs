namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// HTTP bağlamından oturum açmış kullanıcının kimlik bilgilerini sağlar.
/// Uygulama API katmanındadır (HttpContext üzerinden).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Oturum açmış kullanıcının kimliği. Anonim isteklerde null.</summary>
    Guid? UserId { get; }

    /// <summary>Kullanıcının rolü (Customer, Admin, Seller). Anonim isteklerde null.</summary>
    string? Role { get; }

    /// <summary>Kullanıcı kimliği doğrulanmış mı?</summary>
    bool IsAuthenticated { get; }
}
