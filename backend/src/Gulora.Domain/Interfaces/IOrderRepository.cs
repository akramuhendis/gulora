using Gulora.Domain.Entities;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Order aggregate root'una özgü sorgu operasyonları.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Sipariş numarasıyla sipariş getirir.</summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>Kullanıcıya ait tüm siparişleri sayfalı getirir.</summary>
    Task<IReadOnlyList<Order>> GetByUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Kullanıcıya ait toplam sipariş sayısını döner (sayfalama için).</summary>
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Belirli durumdaki siparişleri getirir (Admin paneli için).</summary>
    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);

    /// <summary>
    /// Belirli tarih aralığındaki siparişleri getirir (raporlama).
    /// </summary>
    Task<IReadOnlyList<Order>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    /// <summary>Günlük son sipariş sıra numarasını döner (sipariş numarası üretimi için).</summary>
    Task<int> GetDailySequenceAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Siparişte verilen kullanıcı kimliğine (<paramref name="sellerUserId"/>) ait en az bir ürün kalemi var mı.
    /// Satıcı paneli sipariş görüntüleme ve kısmi yetki kontrolleri için kullanılır.
    /// </summary>
    Task<bool> OrderInvolvesSellerAsync(Guid orderId, Guid sellerUserId, CancellationToken ct = default);

    /// <summary>
    /// Siparişteki tüm kalemler bu satıcıya ait ürünlerden mi (durum güncelleme / kargo için tam yetki).
    /// Sepette birden fazla satıcı varsa false döner.
    /// </summary>
    Task<bool> OrderIsFullyOwnedBySellerAsync(Guid orderId, Guid sellerUserId, CancellationToken ct = default);

    /// <summary>Satıcının ürünlerini içeren siparişleri sayfalı listeler.</summary>
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedForSellerAsync(
        Guid sellerUserId,
        OrderStatus? status,
        string? orderNumber,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Returns per-order sum of line totals for products owned by the seller.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetSellerAttributedTotalsByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        Guid sellerUserId,
        CancellationToken ct = default);

    /// <summary>Satıcı icmalı — tam kəmiyyət, gün buckletləri və son sifarişlər.</summary>
    Task<SellerDashboardOrderSnapshot> GetSellerDashboardOrderSnapshotAsync(
        Guid sellerUserId,
        int recentOrderTake,
        int trendDayCount,
        CancellationToken ct = default);
}
