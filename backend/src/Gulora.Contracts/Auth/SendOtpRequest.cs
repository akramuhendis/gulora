namespace Gulora.Contracts.Auth;

/// <summary>
/// POST api/v1/auth/otp/send gövdesi.
/// Belirtilen telefon numarasına SMS ile tek kullanımlık kod (OTP) gönderir.
/// Rate-limit: aynı numaraya kısa sürede tekrar istek atılmasına izin verilmez (429).
/// </summary>
public sealed record SendOtpRequest(
    string PhoneNumber);
