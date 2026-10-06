namespace Gulora.Contracts.Reviews;

/// <summary>
/// PUT api/v1/products/{productId}/reviews/{id} gövdesi.
/// Kullanıcı kendi yorumunu günceller; güncelleme sonrası yorum <c>Pending</c> durumuna döner
/// ve yeniden moderasyon onayı bekler (domain <c>Review.Update</c> davranışı).
/// </summary>
public sealed record UpdateReviewRequest(
    int Rating,
    string? Title = null,
    string? Body = null);
