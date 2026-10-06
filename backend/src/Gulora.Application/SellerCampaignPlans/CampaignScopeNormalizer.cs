using System.Text.Json;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;

namespace Gulora.Application.SellerCampaignPlans;

/// <summary>Əhatə JSON-unu saxlama formatına normallaşdırır və satıcıya aid məhsulları yoxlayır.</summary>
public static class CampaignScopeNormalizer
{
    private static readonly JsonSerializerOptions JsonWrite = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<string> NormalizeAsync(
        JsonElement scope,
        Guid sellerUserId,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        CancellationToken ct)
    {
        if (scope.ValueKind != JsonValueKind.Object)
            throw new BusinessRuleException("Əhatə etibarlı JSON obyekti olmalıdır.");

        if (!scope.TryGetProperty("type", out var typeEl) || typeEl.ValueKind != JsonValueKind.String)
            throw new BusinessRuleException("Əhatədə 'type' sahəsi tələb olunur.");

        var t = typeEl.GetString()?.Trim().ToLowerInvariant();
        return t switch
        {
            "all" => """{"type":"all"}""",
            "category" => await NormalizeCategoryAsync(scope, categoryRepository, ct),
            "products" => await NormalizeProductsAsync(scope, sellerUserId, productRepository, ct),
            _ => throw new BusinessRuleException("Əhatə növü 'all', 'category' və ya 'products' olmalıdır.")
        };
    }

    private static async Task<string> NormalizeCategoryAsync(
        JsonElement scope,
        ICategoryRepository categoryRepository,
        CancellationToken ct)
    {
        if (!scope.TryGetProperty("categoryId", out var idEl) || idEl.ValueKind != JsonValueKind.String)
            throw new BusinessRuleException("Kateqoriya əhatəsi üçün 'categoryId' tələb olunur.");

        var idStr = idEl.GetString()?.Trim();
        if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(idStr, out var categoryId))
            throw new BusinessRuleException("categoryId etibarlı GUID olmalıdır.");

        var cat = await categoryRepository.GetByIdAsync(categoryId, ct)
            ?? throw new NotFoundException("Kateqoriya", categoryId);

        return JsonSerializer.Serialize(
            new { type = "category", categoryId = categoryId.ToString(), categoryName = cat.Name },
            JsonWrite);
    }

    private static async Task<string> NormalizeProductsAsync(
        JsonElement scope,
        Guid sellerUserId,
        IProductRepository productRepository,
        CancellationToken ct)
    {
        if (!scope.TryGetProperty("products", out var arr) || arr.ValueKind != JsonValueKind.Array)
            throw new BusinessRuleException("Məhsul əhatəsi üçün 'products' massivi tələb olunur.");

        var ids = new List<Guid>();
        foreach (var item in arr.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;
            if (!item.TryGetProperty("id", out var idEl))
                continue;
            if (idEl.ValueKind == JsonValueKind.String)
            {
                var s = idEl.GetString()?.Trim();
                if (!string.IsNullOrEmpty(s) && Guid.TryParse(s, out var g))
                    ids.Add(g);
            }
        }

        if (ids.Count == 0)
            throw new BusinessRuleException("Ən azı bir etibarlı məhsul ID seçilməlidir.");

        var distinct = ids.Distinct().ToList();
        var rows = await productRepository.ListProductNamesForSellerAsync(sellerUserId, distinct, ct);
        if (rows.Count != distinct.Count)
            throw new BusinessRuleException("Bəzi məhsul ID-ləri sizə aid deyil və ya tapılmadı.");

        var ordered = distinct
            .Select(id => rows.First(r => r.Id == id))
            .Select(r => new { id = r.Id.ToString(), name = r.Name })
            .ToList();

        return JsonSerializer.Serialize(new { type = "products", products = ordered }, JsonWrite);
    }
}
