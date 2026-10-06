using Gulora.API.Extensions;
using Gulora.Application.Seller.Queries.GetSellerDashboard;
using Gulora.Contracts.Seller;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>Satıcı icmal — aqreqat göstəricilər və son sifarişlər.</summary>
[ApiController]
[Route("api/v1/seller/dashboard")]
[Produces("application/json")]
[Authorize(Roles = "Seller")]
public sealed class SellerDashboardController : ControllerBase
{
    private readonly ISender _mediator;

    public SellerDashboardController(ISender mediator) => _mediator = mediator;

    /// <summary>Tam kəmiyyət üzrə göstəricilər (server hesablanır).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(SellerDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int recentOrders = 8,
        [FromQuery] int trendDays = 14,
        [FromQuery] int lowStockThreshold = 10,
        CancellationToken ct = default)
    {
        var sellerId = User.GetUserId();
        var result = await _mediator.Send(
            new GetSellerDashboardQuery(sellerId, recentOrders, trendDays, lowStockThreshold),
            ct);
        return Ok(result);
    }
}
