using Gulora.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gulora.Infrastructure.Services.Email;

/// <summary>
/// SMTP kapalıyken (<c>Email:Enabled=false</c>) e-posta uçlarını sessizce tamamlar;
/// kayıt ve diğer akışlar yerel geliştirmede başarısız olmaz.
/// </summary>
internal sealed class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger) => _logger = logger;

    public Task SendEmailVerificationAsync(
        string toEmail,
        string fullName,
        string verificationToken,
        CancellationToken ct = default)
    {
        LogSkipped(nameof(SendEmailVerificationAsync), toEmail);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string toEmail,
        string fullName,
        string resetToken,
        CancellationToken ct = default)
    {
        LogSkipped(nameof(SendPasswordResetAsync), toEmail);
        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationAsync(
        string toEmail,
        string fullName,
        string orderNumber,
        decimal totalAmount,
        CancellationToken ct = default)
    {
        LogSkipped(nameof(SendOrderConfirmationAsync), toEmail);
        return Task.CompletedTask;
    }

    public Task SendOrderStatusChangedAsync(
        string toEmail,
        string fullName,
        string orderNumber,
        string newStatus,
        CancellationToken ct = default)
    {
        LogSkipped(nameof(SendOrderStatusChangedAsync), toEmail);
        return Task.CompletedTask;
    }

    public Task SendCorporateInquiryAsync(
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
        _logger.LogInformation(
            "E-posta devre dışı (Email:Enabled=false); kurumsal talep iletilmedi. Şirket: {Company}",
            company);
        return Task.CompletedTask;
    }

    private void LogSkipped(string method, string toEmail) =>
        _logger.LogInformation(
            "E-posta devre dışı (Email:Enabled=false); {Method} atlandı. Alıcı: {To}",
            method,
            toEmail);
}
