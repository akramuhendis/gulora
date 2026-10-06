namespace Gulora.Domain.Exceptions;

/// <summary>
/// Tekrarlanan kayıt veya çakışan kaynak durumunda fırlatılır (ör. aynı e-posta).
/// HTTP 409 ile eşleştirilir.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string entityName, string field, string value)
        : base($"'{entityName}' için '{field}' alanında '{value}' zaten mevcut.") { }
}
