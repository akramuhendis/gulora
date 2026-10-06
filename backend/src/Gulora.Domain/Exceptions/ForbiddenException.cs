namespace Gulora.Domain.Exceptions;

/// <summary>
/// Kimlik doğrulanmış kullanıcı istenen kaynağa erişim yetkisine sahip değilse fırlatılır.
/// HTTP 403 ile eşleştirilir.
/// Kaynak düzeyi yetki kontrolü (ör. "bu sipariş bana ait mi?") Application handler'da yapılır.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Bu işlem için yetkiniz yok.")
        : base(message) { }
}
