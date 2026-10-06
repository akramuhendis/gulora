namespace Gulora.Contracts.Auth;

/// <summary>
/// POST api/v1/auth/verify-email gövdesi.
/// Kullanıcı kayıt sonrası e-postasına gelen tek kullanımlık token ile
/// e-posta adresini doğrular; token süresi dolmuşsa 400 döner.
/// </summary>
public sealed record VerifyEmailRequest(
    string Token);
