namespace Gulora.Contracts.Auth;

/// <summary>
/// OTP gönderme / doğrulama yanıtı.
/// IsVerified: yalnızca VerifyOtp ucu için anlamlıdır; Send ucunda her zaman false olur.
/// ExpiresAt: OTP'nin geçerlilik süresi (UTC) — istemci sayaç göstermek için kullanabilir.
/// </summary>
public sealed record OtpResponse(
    bool IsVerified,
    string? Message = null,
    DateTime? ExpiresAt = null);
