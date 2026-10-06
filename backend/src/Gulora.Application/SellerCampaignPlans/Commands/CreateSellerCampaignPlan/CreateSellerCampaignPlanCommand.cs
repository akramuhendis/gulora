using System.Text.Json;
using Gulora.Contracts.SellerCampaigns;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Commands.CreateSellerCampaignPlan;

public sealed record CreateSellerCampaignPlanCommand(
    string Name,
    string Kind,
    int Percent,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal? MinCartAmount,
    JsonElement Scope,
    string? Notes) : IRequest<SellerCampaignPlanResponse>;
