namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// Dosya yükleme ve depolama arayüzü.
/// Uygulama Infrastructure katmanındadır (disk, object storage vb.).
/// Güvenlik: izin verilen MIME/boyut doğrulaması Infrastructure'da, politika gulora-security.mdc.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Dosyayı belirtilen klasöre yükler ve depolanmış URL döner.
    /// </summary>
    /// <param name="folder">Alt klasör adı (ör. "products", "avatars").</param>
    /// <param name="fileName">Benzersiz dosya adı (rastgele/Guid tabanlı önerilir).</param>
    /// <param name="content">Dosya içeriği.</param>
    /// <param name="contentType">MIME tipi (ör. "image/jpeg").</param>
    Task<string> UploadAsync(
        string folder,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken ct = default);

    /// <summary>Dosyayı siler. Bulunamazsa sessizce geçer.</summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}
