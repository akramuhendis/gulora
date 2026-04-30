using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// JWT (erişim belirteci) üretim sözleşmesi. Kimlik bilgileri <see cref="User"/>
/// üzerinden; yenileme token'ı rast kripto üretimidir. Gerçek imzalama Infrastructure/Identity’de.
/// </summary>
public interface IJwtService
{
    /// <summary>OAuth benzeri kısa ömürlü <c>access</c> token: talep/claim'lerle üretim.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Refresh token: uzun ömürlü, yüksek entropi rast değer (düz metin saklanmaz, hash/şifreli hali repo’da olabilir).</summary>
    string GenerateRefreshToken();
}
