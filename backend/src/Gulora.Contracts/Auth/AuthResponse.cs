namespace Gulora.Contracts.Auth;

/// <summary>
/// Login / Register / RefreshToken başarılı yanıtı.
/// AccessToken kısa ömürlü (15 dk); RefreshToken httpOnly cookie veya body ile iletilir.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiry,
    AuthUserResponse User);

/// <summary>Token yanıtına gömülen kullanıcı özet bilgisi.</summary>
public sealed record AuthUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role);
