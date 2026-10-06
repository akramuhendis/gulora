namespace Gulora.Domain.Exceptions;

/// <summary>
/// İstenen kaynak sistemde bulunamadığında fırlatılır.
/// HTTP 404 ile eşleştirilir — API exception middleware bu türü yakalar.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"'{entityName}' kaydı '{key}' kimliğiyle bulunamadı.") { }

    public NotFoundException(string message) : base(message) { }
}
