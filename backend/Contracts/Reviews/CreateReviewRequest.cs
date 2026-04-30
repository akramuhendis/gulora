namespace Gulora.Contracts.Reviews;

public record CreateReviewRequest(Guid ProductId, int Rating, string Comment);
