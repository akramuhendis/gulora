using System.Text.Json;
using System.Text.Json.Nodes;

namespace Gulora.Contracts.SellerCampaigns;

/// <summary>GET api/v1/seller-shops/view/{slug}/campaigns — aktiv kampaniya (ictimai vitrin).</summary>
public sealed record SellerCampaignPlanPublicResponse(
    string Name,
    string Kind,
    int Percent,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal? MinCartAmount,
    JsonNode? Scope);

/// <summary>GET api/v1/seller-shops/active-campaigns — bütün mağazalar üçün aktiv planlar (ana səhifə).</summary>
public sealed record SellerCampaignPlanPublicHomeResponse(
    string StoreSlug,
    string StoreName,
    string Name,
    string Kind,
    int Percent,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal? MinCartAmount,
    JsonNode? Scope);

/// <summary>GET /api/v1/seller/campaigns cavab elementi.</summary>
public sealed record SellerCampaignPlanResponse(
    Guid Id,
    string Name,
    string Kind,
    int Percent,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal? MinCartAmount,
    /// <summary>JSON obyekti — JsonElement əvəzinə (serileştirmə / DB uyğunluğu).</summary>
    JsonNode? Scope,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>POST /api/v1/seller/campaigns</summary>
public sealed record CreateSellerCampaignPlanRequest(
    string Name,
    string Kind,
    int Percent,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal? MinCartAmount,
    JsonElement Scope,
    string? Notes);

/// <summary>PUT /api/v1/seller/campaigns/{id}</summary>
public sealed record UpdateSellerCampaignPlanRequest(
    string Name,
    string Kind,
    int Percent,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal? MinCartAmount,
    JsonElement Scope,
    string? Notes);
