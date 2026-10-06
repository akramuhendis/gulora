using Gulora.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Twilio.Clients;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Gulora.Infrastructure.Services.Sms;

/// <summary>
/// SMS OTP — Twilio yapılandırılmışsa gerçek gönderim; aksi halde güvenli fallback log.
/// Twilio: AccountSid, AuthToken ve (From veya MessagingServiceSid) zorunlu.
/// Güvenlik: kod yalnızca Sms:LogPlainCodeInDevelopment=true iken loglanır.
/// </summary>
internal sealed class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<SmsService> _logger;

    public SmsService(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendOtpAsync(string phoneNumber, string plainCode, CancellationToken ct = default)
    {
        var template = _configuration["Sms:OtpBodyTemplate"] ?? "Gülora doğrulama kodunuz: {0}";
        var body = string.Format(template, plainCode);

        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken = _configuration["Twilio:AuthToken"];
        var from = _configuration["Twilio:From"];
        var messagingServiceSid = _configuration["Twilio:MessagingServiceSid"];

        if (!string.IsNullOrWhiteSpace(accountSid)
            && !string.IsNullOrWhiteSpace(authToken)
            && (!string.IsNullOrWhiteSpace(messagingServiceSid)
                || !string.IsNullOrWhiteSpace(from)))
        {
            await SendViaTwilioAsync(
                phoneNumber,
                body,
                accountSid.Trim(),
                authToken.Trim(),
                string.IsNullOrWhiteSpace(messagingServiceSid) ? null : messagingServiceSid.Trim(),
                string.IsNullOrWhiteSpace(from) ? null : from.Trim(),
                ct);
            return;
        }

        if (!string.IsNullOrWhiteSpace(accountSid) || !string.IsNullOrWhiteSpace(authToken))
        {
            _logger.LogWarning(
                "Twilio AccountSid veya AuthToken kısmen tanımlı; eksik From/MessagingServiceSid veya kimlik bilgisi.");
        }

        FallbackLogOnly(phoneNumber, plainCode);
    }

    private async Task SendViaTwilioAsync(
        string phoneNumber,
        string body,
        string accountSid,
        string authToken,
        string? messagingServiceSid,
        string? from,
        CancellationToken ct)
    {
        var client = new TwilioRestClient(accountSid, authToken);

        try
        {
            MessageResource msg;
            if (messagingServiceSid is not null)
            {
                msg = await MessageResource.CreateAsync(
                        body: body,
                        messagingServiceSid: messagingServiceSid,
                        to: new PhoneNumber(phoneNumber),
                        client: client)
                    .WaitAsync(ct);
            }
            else
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(from);
                msg = await MessageResource.CreateAsync(
                        body: body,
                        from: new PhoneNumber(from),
                        to: new PhoneNumber(phoneNumber),
                        client: client)
                    .WaitAsync(ct);
            }

            _logger.LogInformation(
                "Twilio SMS gönderildi. Sid={Sid}, alıcı son haneler: {Hint}",
                msg.Sid,
                MaskPhone(phoneNumber));
        }
        catch (ApiException ex)
        {
            _logger.LogError(
                ex,
                "Twilio SMS başarısız. Status={Status}, Code={Code}, alıcı={Hint}",
                ex.Status,
                ex.Code,
                MaskPhone(phoneNumber));

            throw new InvalidOperationException(
                "SMS doğrulama kodu gönderilemedi. Lütfen daha sonra tekrar deneyin.",
                ex);
        }
    }

    private void FallbackLogOnly(string phoneNumber, string plainCode)
    {
        if (_environment.IsDevelopment()
            && _configuration.GetValue("Sms:LogPlainCodeInDevelopment", false))
        {
            _logger.LogWarning(
                "SMS OTP (Twilio yok — Sms:LogPlainCodeInDevelopment=true): {Phone} kod={Code}",
                MaskPhone(phoneNumber),
                plainCode);
        }
        else
        {
            _logger.LogInformation(
                "SMS OTP iletimi (Twilio yapılandırılmadı; yalnızca log). Alıcı: {Hint}",
                MaskPhone(phoneNumber));
        }
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
            return "***";
        return "..." + phone[^4..];
    }
}
