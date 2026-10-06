using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// JWT access token ve refresh token üretim/doğrulama arayüzü.
/// Uygulama Infrastructure katmanındadır.
/// AccessToken süresi 15 dk; RefreshToken 7 gün DB'de saklanır.
/// Token asla loglanmaz (gulora-security.mdc).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Kullanıcı için kısa ömürlü JWT access token üretir.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Rastgele refresh token değeri üretir (DB'de saklanacak).</summary>
    string GenerateRefreshToken();

    /// <summary>Access token'dan kullanıcı kimliğini okur (geçersiz/süresi dolmuşsa null).</summary>
    Guid? GetUserIdFromToken(string accessToken);
}
