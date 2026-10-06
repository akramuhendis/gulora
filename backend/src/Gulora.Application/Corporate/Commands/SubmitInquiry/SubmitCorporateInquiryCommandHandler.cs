using Gulora.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gulora.Application.Corporate.Commands.SubmitInquiry;

/// <summary>
/// Kurumsal talep e-postasını admin adresine iletir.
/// İleride CRM entegrasyonu veya ticket sistemi eklendiğinde bu handler güncellenir.
/// </summary>
public sealed class SubmitCorporateInquiryCommandHandler : IRequestHandler<SubmitCorporateInquiryCommand>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SubmitCorporateInquiryCommandHandler> _logger;

    public SubmitCorporateInquiryCommandHandler(
        IEmailService emailService,
        ILogger<SubmitCorporateInquiryCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(SubmitCorporateInquiryCommand cmd, CancellationToken ct)
    {
        // Talep alındığı loglanır; e-posta PII olduğu için loglanmaz (gulora-security.mdc).
        _logger.LogInformation(
            "Kurumsal talep alındı. Şirket: {Company}, İletişim: {ContactName}",
            cmd.Company, cmd.ContactName);

        await _emailService.SendCorporateInquiryAsync(
            cmd.Company,
            cmd.ContactName,
            cmd.Email,
            cmd.Phone,
            cmd.Occasion,
            cmd.Quantity,
            cmd.Budget,
            cmd.Message,
            ct);
    }
}
