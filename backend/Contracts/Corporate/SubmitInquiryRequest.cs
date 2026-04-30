namespace Gulora.Contracts.Corporate;

public record SubmitInquiryRequest(
    string Company,
    string ContactName,
    string Email,
    string Phone,
    string Message,
    string? Occasion,
    int? Quantity,
    decimal? Budget);
