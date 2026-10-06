using Gulora.API.Extensions;
using Gulora.Application.Products.Commands.ActivateProduct;
using Gulora.Application.Products.Commands.AddProductImage;
using Gulora.Application.Products.Commands.CreateProduct;
using Gulora.Application.Products.Commands.DeleteProduct;
using Gulora.Application.Products.Commands.UpdateProduct;
using Gulora.Application.Products.Commands.UpdateProductPrice;
using Gulora.Application.Products.Commands.UpdateProductStock;
using Gulora.Application.Products.Queries.GetProductBySlug;
using Gulora.Application.Products.Queries.GetProducts;
using Gulora.Application.Products.Queries.SearchProducts;
using Gulora.Application.Reviews.Commands.SubmitReview;
using Gulora.Application.Reviews.Commands.UpdateReview;
using Gulora.Application.Reviews.Queries.GetProductReviews;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using Gulora.Contracts.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Ürün kataloğu, arama ve ürüne ait yorumlar.
/// Listeleme ve detay uçları herkese açıktır; yönetim uçları Admin/Seller rolü gerektirir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProductsController(ISender mediator) => _mediator = mediator;

    // ── Listeleme ve arama ──────────────────────────────────────────────────────

    /// <summary>Ürünleri sayfali listeler; opsiyonel kategori filtresi destekler.</summary>
    /// <response code="200">Sayfalı ürün listesi.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] GetProductsRequest req,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(
                req.CategoryId,
                req.SellerId,
                req.Brand,
                req.MinPrice,
                req.MaxPrice,
                req.IsFeatured,
                req.InStock,
                req.SortBy,
                req.Page,
                req.PageSize,
                req.OnPromotion),
            ct);
        return Ok(result);
    }

    /// <summary>Anahtar kelimeyle ürün arar (ad, açıklama).</summary>
    /// <response code="200">Sayfalı arama sonuçları.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] SearchProductsRequest req, CancellationToken ct)
    {
        var keyword = req.Query ?? string.Empty;
        var result = await _mediator.Send(
            new SearchProductsQuery(
                keyword,
                req.Page,
                req.PageSize,
                req.CategoryId,
                req.MinPrice,
                req.MaxPrice,
                req.InStock),
            ct);
        return Ok(result);
    }

    /// <summary>Slug ile ürün detayını getirir.</summary>
    /// <response code="200">Ürün detayı.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductBySlugQuery(slug), ct);
        return Ok(result);
    }

    // ── Yönetim (Admin / Seller) ─────────────────────────────────────────────

    /// <summary>Yeni ürün oluşturur.</summary>
    /// <response code="201">Oluşturulan ürünün ID'si.</response>
    /// <response code="400">Doğrulama hatası.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateProductCommand(
            req.Name,
            req.Slug,
            req.Price,
            req.StockQuantity,
            req.CategoryId,
            req.Description,
            req.ShortDescription,
            req.Sku,
            req.Brand,
            req.DiscountedPrice,
            req.IsFeatured), ct);

        return CreatedAtAction(nameof(GetBySlug), new { slug = req.Slug }, id);
    }

    /// <summary>Ürün bilgilerini günceller.</summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateProductCommand(
            id,
            req.Name,
            req.Slug,
            req.Description,
            req.ShortDescription,
            req.Brand,
            req.CategoryId,
            req.IsFeatured), ct);
        return NoContent();
    }

    /// <summary>Ürünü devre dışı bırakır (soft-delete).</summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }

    /// <summary>Ürünü tekrar aktif eder.</summary>
    /// <response code="204">Aktifleştirme başarılı.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateProduct(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateProductCommand(id), ct);
        return NoContent();
    }

    /// <summary>Ürün fiyatını ve opsiyonel indirimli fiyatı günceller.</summary>
    /// <response code="204">Fiyat güncellendi.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpPatch("{id:guid}/price")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(
        Guid id, [FromBody] UpdateProductPriceRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateProductPriceCommand(
            id, req.Price, req.DiscountedPrice), ct);
        return NoContent();
    }

    /// <summary>Ürün stok miktarını günceller.</summary>
    /// <response code="204">Stok güncellendi.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="404">Ürün bulunamadı.</response>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(
        Guid id, [FromBody] UpdateProductStockRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateProductStockCommand(id, req.StockQuantity), ct);
        return NoContent();
    }

    /// <summary>Ürüne görsel URL'si ekler (en fazla 10).</summary>
    /// <response code="201">Oluşturulan görsel kaydı ID'si.</response>
    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductImage(
        Guid id, [FromBody] AddProductImageRequest req, CancellationToken ct)
    {
        var imageId = await _mediator.Send(
            new AddProductImageCommand(id, req.Url, req.AltText, req.DisplayOrder), ct);
        return StatusCode(StatusCodes.Status201Created, imageId);
    }

    // ── Yorumlar (ürüne bağlı) ───────────────────────────────────────────────

    /// <summary>Ürüne ait onaylı yorumları sayfali getirir.</summary>
    /// <response code="200">Sayfalı yorum listesi.</response>
    [HttpGet("{productId:guid}/reviews")]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(
        Guid productId,
        [FromQuery] GetProductReviewsRequest req,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductReviewsQuery(productId, req.Page, req.PageSize), ct);
        return Ok(result);
    }

    /// <summary>Ürüne yorum gönderir. Onay bekler (Pending).</summary>
    /// <response code="201">Yorum oluşturuldu.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="409">Bu ürün için zaten yorum yapılmış.</response>
    [HttpPost("{productId:guid}/reviews")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitReview(
        Guid productId,
        [FromBody] CreateReviewRequest req,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var id = await _mediator.Send(new SubmitReviewCommand(
            productId, userId, req.Rating, req.Title, req.Body, req.OrderId), ct);

        return CreatedAtAction(nameof(GetReviews), new { productId }, id);
    }

    /// <summary>Kullanıcı kendi yorumunu günceller; yorum yeniden Pending durumuna döner.</summary>
    /// <response code="204">Yorum güncellendi.</response>
    /// <response code="400">Doğrulama hatası.</response>
    /// <response code="403">Başkasının yorumu düzenlenemez.</response>
    /// <response code="404">Yorum bulunamadı.</response>
    [HttpPut("{productId:guid}/reviews/{reviewId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(
        Guid productId,
        Guid reviewId,
        [FromBody] UpdateReviewRequest req,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new UpdateReviewCommand(
            reviewId, userId, req.Rating, req.Title, req.Body), ct);
        return NoContent();
    }
}
