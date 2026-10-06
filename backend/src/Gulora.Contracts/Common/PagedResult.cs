namespace Gulora.Contracts.Common;

/// <summary>
/// Sayfalanmış sorgu yanıtı — tüm liste endpoint'lerinde kullanılır.
/// JSON: items, totalCount, page, pageSize (camelCase serileştirme API projesinde yapılandırılır).
/// </summary>
public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>Toplam sayfa sayısı.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>Önceki sayfa var mı?</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Sonraki sayfa var mı?</summary>
    public bool HasNextPage => Page < TotalPages;
}
