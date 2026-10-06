using MediatR;

namespace Gulora.Application.Reviews.Commands.SubmitReview;

/// <summary>Yeni ürün yorumu gönderir. Başarıda yorum Guid'i döner.</summary>
public sealed record SubmitReviewCommand(
    Guid ProductId,
    Guid UserId,
    int Rating,
    string? Title = null,
    string? Body = null,
    Guid? OrderId = null
) : IRequest<Guid>;
