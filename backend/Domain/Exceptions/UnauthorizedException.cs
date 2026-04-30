namespace Gulora.Domain.Exceptions;

/// <summary>
/// Kimlik doğrulama veya yetkilendirme başarısız olduğunda kullanılır; genelde HTTP 401/403 ile eşlenir.
/// </summary>
/// <param name="message">Hata açıklaması; varsayılan genel yetkisiz mesajıdır.</param>
public class UnauthorizedException(string message = "İcazə yoxdur.") : Exception(message);
