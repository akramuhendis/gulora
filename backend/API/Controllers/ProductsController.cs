using Gulora.Application.Products.Queries.GetProductBySlug;
using Gulora.Application.Products.Queries.GetProducts;
using Gulora.Application.Products.Queries.SearchProducts;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get([FromQuery] GetProductsRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsQuery(request), ct);
        return Ok(result);
    }

    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductBySlugQuery(slug), ct);
        return Ok(result);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new SearchProductsQuery(q, limit), ct);
        return Ok(result);
    }
}
