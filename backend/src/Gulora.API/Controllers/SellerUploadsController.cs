using Gulora.Contracts.Uploads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulora.API.Controllers;

/// <summary>
/// Satıcı / Admin üçün məhsul kapak şəkli yükləmə — diskdə saxlanır, mütləq http(s) URL qaytarır
/// (<see cref="AddProductImageRequest"/> doğrulaması ilə uyğun).
/// </summary>
[ApiController]
[Route("api/v1/seller/uploads")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Seller")]
public sealed class SellerUploadsController : ControllerBase
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExt =
        new(new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }, StringComparer.OrdinalIgnoreCase);

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SellerUploadsController> _logger;

    public SellerUploadsController(IWebHostEnvironment env, ILogger<SellerUploadsController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>Multipart şəkil faylı qəbul edir, qovluğa yazır və tam URL qaytarır.</summary>
    [HttpPost("product-cover")]
    [RequestSizeLimit(MaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytes)]
    [ProducesResponseType(typeof(UploadProductCoverResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProductCover(IFormFile? file, CancellationToken ct)
    {
        return await UploadImageAsync(
            file,
            "products",
            "/uploads/products/",
            "Kapak şəkli",
            ct);
    }

    /// <summary>Satıcı mağaza profili üçün logo yükləyir və tam URL qaytarır.</summary>
    [HttpPost("shop-logo")]
    [RequestSizeLimit(MaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBytes)]
    [ProducesResponseType(typeof(UploadProductCoverResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadShopLogo(IFormFile? file, CancellationToken ct)
    {
        return await UploadImageAsync(
            file,
            Path.Combine("shops", "logos"),
            "/uploads/shops/logos/",
            "Mağaza loqosu",
            ct);
    }

    private async Task<IActionResult> UploadImageAsync(
        IFormFile? file,
        string folderRelativeToUploads,
        string publicPathPrefix,
        string logLabel,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Fayl yoxdur",
                Detail = "Multipart gövdədə \"file\" adlı şəkil faylı göndərin.",
            });
        }

        if (file.Length > MaxBytes)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Fayl çox böyükdür",
                Detail = $"Maksimum ölçü {MaxBytes / 1024 / 1024} MB.",
            });
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dəstəklənməyən format",
                Detail = "Yalnız .jpg, .jpeg, .png, .webp, .gif qəbul olunur.",
            });
        }

        var uploadsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", folderRelativeToUploads);
        Directory.CreateDirectory(uploadsDir);

        var storedName = $"{Guid.NewGuid():n}{ext}";
        var physicalPath = Path.Combine(uploadsDir, storedName);

        try
        {
            await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(stream, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Label} yazılmadı: {Path}", logLabel, physicalPath);
            return Problem(
                detail: "Fayl diskə yazıla bilmədi.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var url = $"{Request.Scheme}://{Request.Host.Value}{publicPathPrefix}{storedName}";

        return Ok(new UploadProductCoverResponse(url));
    }
}
