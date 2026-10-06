using System.ComponentModel.DataAnnotations;

namespace Gulora.Contracts.Common;

/// <summary>
/// Sayfalanmış liste sorgularının temel parametreleri.
/// pageSize üst sınırı: 100 (FluentValidation veya controller'da uygulanır).
/// </summary>
public record PagedRequest
{
    /// <summary>Sayfa numarası (1'den başlar).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den küçük olamaz.")]
    public int Page { get; init; } = 1;

    /// <summary>Sayfa başına kayıt sayısı (1–100 arası).</summary>
    [Range(1, PagingConstraints.MaxPageSize, ErrorMessage = "Sayfa boyutu 1 ile 100 arasında olmalıdır.")]
    public int PageSize { get; init; } = PagingConstraints.DefaultPageSize;
}
