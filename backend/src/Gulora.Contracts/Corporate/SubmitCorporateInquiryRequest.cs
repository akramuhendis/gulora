namespace Gulora.Contracts.Corporate;

/// <summary>
/// POST api/v1/corporate/inquiries gövdesi — kurumsal / B2B talep formu.
/// Kimlik doğrulama zorunlu değildir; misafir kullanıcılar da gönderebilir.
/// Backend CRM veya dahili bildirim kanalına iletir; tekrarlı gönderimleri
/// önlemek için rate-limit ve idempotency uygulanması önerilir.
/// </summary>
public sealed record SubmitCorporateInquiryRequest(
    string Company,
    string ContactName,
    string Email,
    string Phone,
    string Occasion,
    string Quantity,
    string Budget,
    string? Message = null);
