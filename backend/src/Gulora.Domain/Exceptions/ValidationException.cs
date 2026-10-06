namespace Gulora.Domain.Exceptions;

/// <summary>
/// Girdi doğrulama kuralı ihlallerini taşır.
/// HTTP 422 veya 400 ile eşleştirilir.
/// Birden fazla alan hatası tek örnekte toplanabilir.
/// </summary>
public sealed class ValidationException : DomainException
{
    /// <summary>Alan adı → hata mesajları sözlüğü.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Bir veya daha fazla doğrulama hatası oluştu.")
    {
        Errors = errors.AsReadOnly();
    }

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] }) { }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
