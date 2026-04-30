using Gulora.API.Extensions;
using Gulora.Application.Reviews.Commands.CreateReview;
using Gulora.Application.Reviews.Queries.GetReviews;
using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReviewsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> List(
        [FromQuery] Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetReviewsQuery(productId, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var cmd = new CreateReviewCommand(request.ProductId, userId, request.Rating, request.Comment);
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }
}
