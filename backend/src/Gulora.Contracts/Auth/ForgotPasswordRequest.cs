namespace Gulora.Contracts.Auth;

/// <summary>
/// POST api/v1/auth/forgot-password gövdesi.
/// Girilen e-posta adresi sistemde kayıtlıysa sıfırlama bağlantısı gönderilir.
/// Kullanıcı varlığı dışarıya sızdırılmaz — hesap bulunsun ya da bulunmasın
/// yanıt her zaman 200 döner (timing saldırısını önlemek için).
/// </summary>
public sealed record ForgotPasswordRequest(
    string Email);
