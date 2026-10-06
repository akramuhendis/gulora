namespace Gulora.Contracts.Auth;

/// <summary>
/// POST api/v1/auth/otp/verify gövdesi.
/// Daha önce gönderilen OTP kodu ile telefon numarasını doğrular.
/// Başarılı olursa telefon numarası hesaba bağlanır veya akış devam eder.
/// </summary>
public sealed record VerifyOtpRequest(
    string PhoneNumber,
    string Code);
