using Gulora.API.Extensions;
using Gulora.Application.Wishlist.Commands.AddToWishlist;
using Gulora.Application.Wishlist.Commands.RemoveFromWishlist;
using Gulora.Application.Wishlist.Queries.GetWishlist;
using Gulora.Contracts.Wishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// İstek listesi — ürün ekleme, çıkarma ve listeleme.
/// Tüm uçlar kimlik doğrulaması gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class WishlistController : ControllerBase
{
    private readonly ISender _mediator;

    public WishlistController(ISender mediator) => _mediator = mediator;

    /// <summary>Kullanıcının istek listesini ürün detaylarıyla getirir.</summary>
    /// <response code="200">İstek listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetWishlistQuery(userId), ct);
        return Ok(result);
    }

    /// <summary>Ürünü istek listesine ekler.</summary>
    /// <response code="204">Ekleme başarılı.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToWishlist(
        [FromBody] AddToWishlistRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new AddToWishlistCommand(userId, req.ProductId), ct);
        return NoContent();
    }

    /// <summary>Ürünü istek listesinden kaldırır.</summary>
    /// <response code="204">Kaldırma başarılı.</response>
    /// <response code="404">Ürün istek listesinde bulunamadı.</response>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new RemoveFromWishlistCommand(userId, productId), ct);
        return NoContent();
    }
}
