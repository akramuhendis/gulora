namespace Gulora.Domain.Exceptions;

/// <summary>
/// Kimlik doğrulama başarısız olduğunda veya token geçersiz/süresi dolduğunda fırlatılır.
/// HTTP 401 ile eşleştirilir.
/// </summary>
public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Giriş yapmanız gerekiyor.")
        : base(message) { }
}
