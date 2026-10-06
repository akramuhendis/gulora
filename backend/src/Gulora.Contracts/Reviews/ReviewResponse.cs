namespace Gulora.Contracts.Reviews;

/// <summary>
/// Ürün yorumu yanıtı — onaylı yorumlar listesi ve moderasyon paneli için kullanılır.
/// Status alanı: Pending | Approved | Rejected.
/// </summary>
public sealed record ReviewResponse(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    Guid? OrderId,
    string? ReviewerName,
    int Rating,
    string? Title,
    string? Body,
    string Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
