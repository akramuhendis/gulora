using Gulora.API.Extensions;
using Gulora.Application.Products.Queries.GetSellerProducts;
using Gulora.Application.Reviews.Queries.GetSellerProductReviews;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using Gulora.Contracts.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>Satıcı envanter listesi — kendi ürünleri (yayından kaldırılmış dahil).</summary>
[ApiController]
[Route("api/v1/seller/products")]
[Produces("application/json")]
[Authorize(Roles = "Seller")]
public sealed class SellerProductsController : ControllerBase
{
    private readonly ISender _mediator;

    public SellerProductsController(ISender mediator) => _mediator = mediator;

    /// <summary>Oturum satıcısına ait ürünleri sayfalı döner.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProducts(
        [FromQuery] GetSellerProductsRequest req,
        CancellationToken ct)
    {
        var sellerId = User.GetUserId();
        var result = await _mediator.Send(
            new GetSellerProductsQuery(
                sellerId,
                req.Page,
                req.PageSize,
                req.Query,
                req.IsActive,
                req.InStock,
                req.SortBy),
            ct);
        return Ok(result);
    }

    /// <summary>Satıcının öz məhsuluna düşən bütün rəylər (Pending / Approved / Rejected).</summary>
    [HttpGet("{productId:guid}/reviews")]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProductReviews(
        Guid productId,
        [FromQuery] GetProductReviewsRequest req,
        CancellationToken ct)
    {
        var sellerId = User.GetUserId();
        var result = await _mediator.Send(
            new GetSellerProductReviewsQuery(sellerId, productId, req.Page, req.PageSize),
            ct);
        return Ok(result);
    }
}
