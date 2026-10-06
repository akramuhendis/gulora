using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>Satıcı kampaniya planları.</summary>
public interface ISellerCampaignPlanRepository : IRepository<SellerCampaignPlan>
{
    Task<IReadOnlyList<SellerCampaignPlan>> ListBySellerAsync(Guid sellerUserId, CancellationToken ct = default);

    /// <summary>Hal-hazırda aktiv (UTC interval daxilində) planlar.</summary>
    Task<IReadOnlyList<SellerCampaignPlan>> ListActiveBySellerAsync(
        Guid sellerUserId,
        DateTime nowUtc,
        CancellationToken ct = default);

    /// <summary>
    /// Aktiv planlar vitrin mağazası ilə (ana səhifə / ümumi siyahı).
    /// Yalnız <c>seller_shops</c> qeydiyyatı olan satıcılar daxil edilir.
    /// </summary>
    Task<IReadOnlyList<(SellerCampaignPlan Plan, string StoreSlug, string StoreName)>> ListActiveWithShopSlugsAsync(
        DateTime nowUtc,
        CancellationToken ct = default);

    Task<SellerCampaignPlan?> GetByIdAndSellerAsync(Guid id, Guid sellerUserId, CancellationToken ct = default);
}
