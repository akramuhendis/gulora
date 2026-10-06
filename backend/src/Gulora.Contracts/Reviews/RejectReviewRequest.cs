namespace Gulora.Contracts.Reviews;

/// <summary>POST api/v1/admin/reviews/{id}/reject gövdesi (Admin moderasyon).</summary>
public sealed record RejectReviewRequest(
    string Reason);
