using Gulora.Contracts.Products;
using Gulora.Domain.Entities;

namespace Gulora.Application.SellerCampaignPlans;

internal static class SellerCampaignBestDiscountResolver
{
    /// <summary>
    /// Kataloq <see cref="Product.EffectivePrice"/> üzərindən ən sərfəli aktiv faiz endirimini seçir.
    /// </summary>
    public static ProductSellerCampaignOffer? ResolveForProduct(
        IReadOnlyList<SellerCampaignPlan> activePlans,
        Product product)
    {
        var basePrice = product.EffectivePrice;
        ProductSellerCampaignOffer? bestDiscount = null;
        var bestFinal = decimal.MaxValue;

        foreach (var plan in activePlans)
        {
            if (!string.Equals(plan.Kind, "discount", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!SellerCampaignScopeMatcher.AppliesToProduct(plan.ScopeJson, product))
                continue;

            var final = Math.Round(basePrice * (100 - plan.Percent) / 100m, 2, MidpointRounding.AwayFromZero);
            if (final >= basePrice)
                continue;

            if (final < bestFinal)
            {
                bestFinal = final;
                bestDiscount = new ProductSellerCampaignOffer(
                    plan.Name,
                    plan.Percent,
                    final,
                    plan.MinCartAmount);
            }
        }

        if (bestDiscount is not null)
            return bestDiscount;

        // Faiz endirimi yoxdursa, çatdırılma planını vitrin rozeti üçün qaytarırıq (qiymət eyni qalır).
        ProductSellerCampaignOffer? bestShipping = null;
        var bestShipPct = -1;
        foreach (var plan in activePlans)
        {
            if (!string.Equals(plan.Kind, "shipping", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!SellerCampaignScopeMatcher.AppliesToProduct(plan.ScopeJson, product))
                continue;

            if (plan.Percent <= bestShipPct)
                continue;

            bestShipPct = plan.Percent;
            bestShipping = new ProductSellerCampaignOffer(
                plan.Name,
                plan.Percent,
                basePrice,
                plan.MinCartAmount,
                "shipping");
        }

        return bestShipping;
    }
}
