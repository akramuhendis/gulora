using Gulora.Domain.Entities;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Ürün yorumları sorgu operasyonları.
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>Ürüne ait onaylanmış yorumları sayfalı getirir.</summary>
    Task<IReadOnlyList<Review>> GetApprovedByProductPagedAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Ürüne ait bütün yorumlar (bütün statuslar) — satıcı paneli.</summary>
    Task<IReadOnlyList<Review>> GetByProductPagedAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Ürünə aid ümumi rəy sayı (statusdan asılı olmayaraq).</summary>
    Task<int> GetTotalCountByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Moderasyon bekleyen yorumları getirir (Admin).</summary>
    Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>Kullanıcının belirli ürüne yorum yapıp yapmadığını kontrol eder.</summary>
    Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId, CancellationToken ct = default);

    /// <summary>Ürüne ait onaylanmış yorum sayısını döner (sayfalama için).</summary>
    Task<int> GetApprovedCountByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Ürünün ortalama puanını ve yorum sayısını döner.</summary>
    Task<(double AverageRating, int ReviewCount)> GetRatingStatsAsync(
        Guid productId,
        CancellationToken ct = default);
}
