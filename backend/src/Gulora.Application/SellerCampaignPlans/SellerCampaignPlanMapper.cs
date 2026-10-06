using System.Text.Json;
using System.Text.Json.Nodes;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Domain.Entities;

namespace Gulora.Application.SellerCampaignPlans;

internal static class SellerCampaignPlanMapper
{
    private const string FallbackScope = """{"type":"all"}""";

    public static SellerCampaignPlanResponse ToResponse(SellerCampaignPlan e)
    {
        var raw = string.IsNullOrWhiteSpace(e.ScopeJson) ? FallbackScope : e.ScopeJson;
        JsonNode? scope;
        try
        {
            scope = JsonNode.Parse(raw);
        }
        catch (JsonException)
        {
            scope = JsonNode.Parse(FallbackScope);
        }

        return new SellerCampaignPlanResponse(
            e.Id,
            e.Name,
            e.Kind,
            e.Percent,
            e.StartsAtUtc,
            e.EndsAtUtc,
            e.MinCartAmount,
            scope,
            e.Notes,
            e.CreatedAt,
            e.UpdatedAt);
    }

    public static SellerCampaignPlanPublicResponse ToPublicResponse(SellerCampaignPlan e)
    {
        var raw = string.IsNullOrWhiteSpace(e.ScopeJson) ? FallbackScope : e.ScopeJson;
        JsonNode? scope;
        try
        {
            scope = JsonNode.Parse(raw);
        }
        catch (JsonException)
        {
            scope = JsonNode.Parse(FallbackScope);
        }

        return new SellerCampaignPlanPublicResponse(
            e.Name,
            e.Kind,
            e.Percent,
            e.StartsAtUtc,
            e.EndsAtUtc,
            e.MinCartAmount,
            scope);
    }

    public static SellerCampaignPlanPublicHomeResponse ToPublicHomeResponse(
        SellerCampaignPlan e,
        string storeSlug,
        string storeName)
    {
        var raw = string.IsNullOrWhiteSpace(e.ScopeJson) ? FallbackScope : e.ScopeJson;
        JsonNode? scope;
        try
        {
            scope = JsonNode.Parse(raw);
        }
        catch (JsonException)
        {
            scope = JsonNode.Parse(FallbackScope);
        }

        return new SellerCampaignPlanPublicHomeResponse(
            storeSlug,
            storeName,
            e.Name,
            e.Kind,
            e.Percent,
            e.StartsAtUtc,
            e.EndsAtUtc,
            e.MinCartAmount,
            scope);
    }
}
