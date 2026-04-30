using Gulora.API.Extensions;
using Gulora.Application.Favorites.Commands.AddFavorite;
using Gulora.Application.Favorites.Commands.RemoveFavorite;
using Gulora.Application.Favorites.Queries.GetFavorites;
using Gulora.Contracts.Favorites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FavoritesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FavoriteProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FavoriteProductDto>>> List(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetFavoritesQuery(userId), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> Add([FromBody] AddFavoriteRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var id = await mediator.Send(new AddFavoriteCommand(userId, request.ProductId), ct);
        return Ok(id);
    }

    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Remove(Guid productId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await mediator.Send(new RemoveFavoriteCommand(userId, productId), ct);
        return Ok();
    }
}
