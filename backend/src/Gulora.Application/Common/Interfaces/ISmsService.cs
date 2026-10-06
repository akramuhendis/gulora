namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// SMS OTP gönderimi — altyapı (Twilio vb.) veya geliştirme modu çıktısı.
/// Kod üretimi Application'dadır; servis yalnızca iletim yapar.
/// </summary>
public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string plainCode, CancellationToken ct = default);
}
