using System.Text.Json;
using Gulora.Domain.Entities;

namespace Gulora.Application.SellerCampaignPlans;

/// <summary>
/// Saxlanmış scope JSON-un məhsula uyğunluğunu yoxlayır (all | category | products).
/// </summary>
internal static class SellerCampaignScopeMatcher
{
    public static bool AppliesToProduct(string scopeJson, Product product)
    {
        // Boş əhatə: domen default-u "all" ilə uyğunlaşdır (köhnə sətirlər / admin fallback).
        if (string.IsNullOrWhiteSpace(scopeJson))
            return true;

        try
        {
            using var doc = JsonDocument.Parse(scopeJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return false;
            if (!root.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.String)
                return false;

            return typeEl.GetString()?.Trim().ToLowerInvariant() switch
            {
                "all" => true,
                "category" => CategoryMatches(root, product.CategoryId),
                "products" => ProductInList(root, product.Id),
                _ => false
            };
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool CategoryMatches(JsonElement root, Guid productCategoryId)
    {
        if (!root.TryGetProperty("categoryId", out var idEl) || idEl.ValueKind != JsonValueKind.String)
            return false;
        var s = idEl.GetString();
        if (string.IsNullOrWhiteSpace(s) || !Guid.TryParse(s.Trim(), out var catId))
            return false;
        return catId == productCategoryId;
    }

    private static bool ProductInList(JsonElement root, Guid productId)
    {
        if (!root.TryGetProperty("products", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return false;

        foreach (var item in arr.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;
            if (!item.TryGetProperty("id", out var idEl))
                continue;
            if (idEl.ValueKind == JsonValueKind.String)
            {
                var s = idEl.GetString();
                if (!string.IsNullOrWhiteSpace(s) && Guid.TryParse(s.Trim(), out var g) && g == productId)
                    return true;
            }
        }

        return false;
    }
}
