namespace Gulora.Contracts.SellerApplications;

/// <summary>
/// Satıcı başvurusu yanıtı.
/// Status geçerli değerleri: Pending | UnderReview | Approved | Rejected.
/// </summary>
public sealed record SellerApplicationResponse(
    Guid Id,
    string FullName,
    string Company,
    string Email,
    string Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
