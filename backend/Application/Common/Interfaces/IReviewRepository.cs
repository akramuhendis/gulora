using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Ürün yorumu (<see cref="Review"/>) sorgu ve ekleme. Paged yorum listesi, tekrar yorumu engellemek
/// için aynı kullanıcı+ürün kontrolü.
/// </summary>
public interface IReviewRepository
{
    /// <summary>Ürün detay sayfası yorumları: sayfalı öğe listesi + toplam adet (sayfa meta için).</summary>
    Task<(IReadOnlyList<Review> Items, int Total)> GetByProductIdAsync(
        Guid productId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Kullanıcının bu ürüne zaten yorum bırakıp bırakmadığını döner; ikinci yorumu engellemek için.</summary>
    Task<bool> HasReviewedAsync(Guid userId, Guid productId, CancellationToken ct = default);

    /// <summary>Yeni yorum satırı ekler.</summary>
    Task AddAsync(Review review, CancellationToken ct = default);

    /// <summary>İnceleme/ürün puan toplamı (ileride) gibi güncellemeleri kaydeder.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
