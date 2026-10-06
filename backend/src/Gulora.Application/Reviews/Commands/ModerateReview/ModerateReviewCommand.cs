using MediatR;

namespace Gulora.Application.Reviews.Commands.ModerateReview;

/// <summary>
/// Yorumu onaylar veya reddeder (Admin moderasyonu).
/// </summary>
public sealed record ModerateReviewCommand(
    Guid ReviewId,
    bool Approve,
    string? RejectionReason = null
) : IRequest;
