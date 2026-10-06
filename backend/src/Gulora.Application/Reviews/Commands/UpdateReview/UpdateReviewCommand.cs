using MediatR;

namespace Gulora.Application.Reviews.Commands.UpdateReview;

/// <summary>
/// Kullanıcı kendi yorumunu günceller.
/// Güncelleme sonrası yorum Pending durumuna döner ve yeniden moderasyon bekler.
/// Kaynak düzeyi yetki: yalnızca yorum sahibi güncelleyebilir (ForbiddenException).
/// </summary>
public sealed record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId,
    int Rating,
    string? Title = null,
    string? Body = null
) : IRequest;
