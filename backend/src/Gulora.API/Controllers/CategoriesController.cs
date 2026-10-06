using Gulora.Application.Categories.Commands.CreateCategory;
using Gulora.Application.Categories.Commands.DeleteCategory;
using Gulora.Application.Categories.Commands.UpdateCategory;
using Gulora.Application.Categories.Queries.GetCategories;
using Gulora.Contracts.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Ürün kategorileri — hiyerarşik ağaç yapısı destekler.
/// Listeleme herkese açıktır; yönetim uçları Admin rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _mediator;

    public CategoriesController(ISender mediator) => _mediator = mediator;

    /// <summary>Tüm kök kategorileri alt kategorileriyle birlikte getirir.</summary>
    /// <param name="onlyActive">Yalnızca aktif kategoriler (varsayılan: true).</param>
    /// <response code="200">Hiyerarşik kategori listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] bool onlyActive = true, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(onlyActive), ct);
        return Ok(result);
    }

    /// <summary>Yeni kategori oluşturur.</summary>
    /// <response code="201">Oluşturulan kategori ID'si.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateCategoryCommand(
            req.Name,
            req.Slug,
            req.Description,
            req.ImageUrl,
            req.ParentId,
            req.DisplayOrder), ct);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    /// <summary>Kategori bilgilerini günceller.</summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="404">Kategori bulunamadı.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid id, [FromBody] UpdateCategoryRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateCategoryCommand(
            id,
            req.Name,
            req.Slug,
            req.Description,
            req.ImageUrl,
            req.DisplayOrder), ct);
        return NoContent();
    }

    /// <summary>Kategoriyi devre dışı bırakır (soft-delete).</summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Kategori bulunamadı.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}
