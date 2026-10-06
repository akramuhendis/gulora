namespace Gulora.Contracts.Reviews;

/// <summary>POST api/v1/products/{productId}/reviews gövdesi.</summary>
public sealed record CreateReviewRequest(
    int Rating,
    string? Title = null,
    string? Body = null,
    Guid? OrderId = null);
