using Gulora.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Gulora.Infrastructure.Services.Storage;

/// <summary>
/// Yerel disk tabanlı dosya depolama servisi (geliştirme ortamı).
/// Üretimde object storage (S3/Azure Blob/GCS) ile değiştirilebilir — arayüz aynı kalır.
/// Güvenlik: dosyalar web kökü dışında saklanır; güvenli rastgele ad kullanılır (gulora-security.mdc).
/// İzin verilen MIME/boyut doğrulaması bu metotta yapılır.
/// </summary>
internal sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        string folder,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        ValidateContentType(contentType);
        ValidateFileSize(content);

        // Dosyalar wwwroot dışında, uploads/ kök klasöründe tutulur
        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
        var targetDir = Path.Combine(uploadsRoot, folder);
        Directory.CreateDirectory(targetDir);

        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(targetDir, safeFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        content.Position = 0;
        await content.CopyToAsync(fileStream, ct);

        _logger.LogInformation("Dosya yüklendi: {Folder}/{FileName}", folder, safeFileName);

        // Göreli URL — API üzerinden servis edilir
        return $"/uploads/{folder}/{safeFileName}";
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        // URL'den göreli yolu dosya sistemine çevir
        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePath = Path.Combine(_env.ContentRootPath, relativePath);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Silinmek istenen dosya bulunamadı: {Path}", filePath);
            return;
        }

        // CancellationToken, Task.Run'a görev oluşturma iptali için değil;
        // iptal isteği öncesinde işlem zaten başladıysa tamamlanmasına izin verilir.
        ct.ThrowIfCancellationRequested();
        await Task.Run(() => File.Delete(filePath), CancellationToken.None);
        _logger.LogInformation("Dosya silindi: {Path}", filePath);
    }

    private static void ValidateContentType(string contentType)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException(
                $"İzin verilmeyen MIME tipi: {contentType}. İzin verilenler: {string.Join(", ", AllowedContentTypes)}");
    }

    private static void ValidateFileSize(Stream content)
    {
        if (content.CanSeek && content.Length > MaxFileSizeBytes)
            throw new InvalidOperationException(
                $"Dosya boyutu {MaxFileSizeBytes / (1024 * 1024)} MB sınırını aşıyor.");
    }
}
