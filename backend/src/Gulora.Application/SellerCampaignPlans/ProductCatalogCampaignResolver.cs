using Gulora.Contracts.Products;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;

namespace Gulora.Application.SellerCampaignPlans;

/// <summary>
/// Kataloq siyahıları üçün aktiv satıcı kampaniyalarını məhsul başına bir dəfə hesablayır.
/// </summary>
internal static class ProductCatalogCampaignResolver
{
    public static async Task<IReadOnlyDictionary<Guid, ProductSellerCampaignOffer?>> ResolveForProductsAsync(
        IReadOnlyList<Product> products,
        ISellerCampaignPlanRepository campaignPlans,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sellerIds = products
            .Select(p => p.SellerId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var activeBySeller = new Dictionary<Guid, IReadOnlyList<SellerCampaignPlan>>(sellerIds.Count);
        foreach (var sid in sellerIds)
        {
            activeBySeller[sid] = await campaignPlans.ListActiveBySellerAsync(sid, now, ct);
        }

        var byProduct = new Dictionary<Guid, ProductSellerCampaignOffer?>(products.Count);
        foreach (var p in products)
        {
            if (p.SellerId is not Guid sid || !activeBySeller.TryGetValue(sid, out var plans))
            {
                byProduct[p.Id] = null;
                continue;
            }

            byProduct[p.Id] = SellerCampaignBestDiscountResolver.ResolveForProduct(plans, p);
        }

        return byProduct;
    }
}
