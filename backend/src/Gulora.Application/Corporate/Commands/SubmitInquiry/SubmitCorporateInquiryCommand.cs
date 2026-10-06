using MediatR;

namespace Gulora.Application.Corporate.Commands.SubmitInquiry;

/// <summary>Kurumsal / B2B talep formu gönderir. E-posta bildirimi tetikler.</summary>
public sealed record SubmitCorporateInquiryCommand(
    string Company,
    string ContactName,
    string Email,
    string Phone,
    string Occasion,
    string Quantity,
    string Budget,
    string? Message = null
) : IRequest;
