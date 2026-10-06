using Gulora.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Gulora.Infrastructure.Services.Email;

/// <summary>
/// MailKit tabanlı e-posta gönderim servisi.
/// SMTP yapılandırması appsettings/user-secrets üzerinden okunur.
/// Şablon içeriği basit HTML string; ölçeklendirmede Razor/Fluid şablona geçilebilir.
/// Aynı olayın iki kez gönderilmesini önlemek için outbox / idempotency key tercih edilir
/// (gulora-infrastructure-contracts.mdc).
/// </summary>
internal sealed class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IConfiguration configuration, ILogger<MailKitEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(
        string toEmail,
        string fullName,
        string verificationToken,
        CancellationToken ct = default)
    {
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "https://gulora.az";
        var link = $"{frontendUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

        var subject = "Gülora — E-posta Adresinizi Doğrulayın";
        var body = $"""
            <p>Merhaba {fullName},</p>
            <p>Hesabınızı doğrulamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href="{link}">{link}</a></p>
            <p>Bu bağlantı 24 saat geçerlidir.</p>
            <p>Teşekkürler,<br/>Gülora Ekibi</p>
            """;

        await SendAsync(toEmail, fullName, subject, body, ct);
    }

    public async Task SendPasswordResetAsync(
        string toEmail,
        string fullName,
        string resetToken,
        CancellationToken ct = default)
    {
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "https://gulora.az";
        var link = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        var subject = "Gülora — Parola Sıfırlama";
        var body = $"""
            <p>Merhaba {fullName},</p>
            <p>Parolanızı sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href="{link}">{link}</a></p>
            <p>Bu bağlantı 1 saat geçerlidir. Talebi siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>
            <p>Teşekkürler,<br/>Gülora Ekibi</p>
            """;

        await SendAsync(toEmail, fullName, subject, body, ct);
    }

    public async Task SendOrderConfirmationAsync(
        string toEmail,
        string fullName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken ct = default)
    {
        var subject = $"Gülora — Siparişiniz Alındı ({orderNumber})";
        var body = $"""
            <p>Merhaba {fullName},</p>
            <p><strong>{orderNumber}</strong> numaralı siparişiniz alındı.</p>
            <p>Toplam Tutar: <strong>{totalAmount:F2} AZN</strong></p>
            <p>Siparişinizin durumunu hesabınızdan takip edebilirsiniz.</p>
            <p>Teşekkürler,<br/>Gülora Ekibi</p>
            """;

        await SendAsync(toEmail, fullName, subject, body, ct);
    }

    public async Task SendOrderStatusChangedAsync(
        string toEmail,
        string fullName,
        string orderNumber,
        string newStatus,
        CancellationToken ct = default)
    {
        var subject = $"Gülora — Sipariş Durumu Güncellendi ({orderNumber})";
        var body = $"""
            <p>Merhaba {fullName},</p>
            <p><strong>{orderNumber}</strong> numaralı siparişinizin durumu güncellendi.</p>
            <p>Yeni Durum: <strong>{newStatus}</strong></p>
            <p>Teşekkürler,<br/>Gülora Ekibi</p>
            """;

        await SendAsync(toEmail, fullName, subject, body, ct);
    }

    public async Task SendCorporateInquiryAsync(
        string company,
        string contactName,
        string contactEmail,
        string phone,
        string occasion,
        string quantity,
        string budget,
        string? message,
        CancellationToken ct = default)
    {
        var adminEmail = _configuration["Email:AdminAddress"]
            ?? throw new InvalidOperationException("Email:AdminAddress yapılandırılmamış.");

        var subject = $"Gülora — Kurumsal Talep: {company}";
        var body = $"""
            <p><strong>Şirket:</strong> {company}</p>
            <p><strong>İletişim:</strong> {contactName} ({contactEmail}, {phone})</p>
            <p><strong>Etkinlik:</strong> {occasion}</p>
            <p><strong>Adet:</strong> {quantity}</p>
            <p><strong>Bütçe:</strong> {budget}</p>
            {(string.IsNullOrWhiteSpace(message) ? "" : $"<p><strong>Not:</strong> {message}</p>")}
            """;

        await SendAsync(adminEmail, "Gülora Admin", subject, body, ct);
    }

    // ──────────────────────────────────────────────────────────────────────────

    private async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken ct)
    {
        var fromEmail = _configuration["Email:FromAddress"]
            ?? throw new InvalidOperationException("Email:FromAddress yapılandırılmamış.");
        var fromName = _configuration["Email:FromName"] ?? "Gülora";
        var host = _configuration["Email:SmtpHost"] ?? "localhost";
        var port = _configuration.GetValue("Email:SmtpPort", 587);
        var username = _configuration["Email:SmtpUsername"];
        var password = _configuration["Email:SmtpPassword"];
        var useSsl = _configuration.GetValue("Email:UseSsl", true);

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
        mimeMessage.To.Add(new MailboxAddress(toName, toEmail));
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart("html") { Text = htmlBody };

        using var smtpClient = new SmtpClient();

        try
        {
            var secureSocketOptions = useSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtpClient.ConnectAsync(host, port, secureSocketOptions, ct);

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                await smtpClient.AuthenticateAsync(username, password, ct);

            await smtpClient.SendAsync(mimeMessage, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderimi başarısız. Alıcı: {To}, Konu: {Subject}",
                toEmail, subject);
            throw;
        }
        finally
        {
            await smtpClient.DisconnectAsync(true, ct);
        }
    }
}
