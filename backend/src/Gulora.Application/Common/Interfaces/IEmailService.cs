namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// E-posta gönderim servisi arayüzü.
/// Uygulama Infrastructure katmanındadır (MailKit / SMTP).
/// Aynı olayın birden fazla gönderilmemesi için outbox veya idempotency key tercih edilir.
/// </summary>
public interface IEmailService
{
    /// <summary>E-posta doğrulama bağlantısı gönderir.</summary>
    Task SendEmailVerificationAsync(
        string toEmail,
        string fullName,
        string verificationToken,
        CancellationToken ct = default);

    /// <summary>Parola sıfırlama bağlantısı gönderir.</summary>
    Task SendPasswordResetAsync(
        string toEmail,
        string fullName,
        string resetToken,
        CancellationToken ct = default);

    /// <summary>Sipariş onay bildirimi gönderir.</summary>
    Task SendOrderConfirmationAsync(
        string toEmail,
        string fullName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken ct = default);

    /// <summary>Sipariş durum değişikliği bildirimi gönderir.</summary>
    Task SendOrderStatusChangedAsync(
        string toEmail,
        string fullName,
        string orderNumber,
        string newStatus,
        CancellationToken ct = default);

    /// <summary>Kurumsal talep formunu admin adresine iletir.</summary>
    Task SendCorporateInquiryAsync(
        string company,
        string contactName,
        string contactEmail,
        string phone,
        string occasion,
        string quantity,
        string budget,
        string? message,
        CancellationToken ct = default);
}
