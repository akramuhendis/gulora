namespace Gulora.Contracts.Auth;

/// <summary>
/// POST api/v1/auth/reset-password gövdesi.
/// ForgotPassword akışından gelen token ile yeni parola belirlenir.
/// Token tek kullanımlıktır ve kısa ömürlüdür (ör. 15–60 dk).
/// İşlem başarılı olursa kullanıcının tüm aktif refresh token'ları iptal edilir.
/// </summary>
public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmNewPassword);
