namespace Gulora.Domain.Exceptions;

/// <summary>
/// Genel iş kuralı ihlali — diğer özel exception türlerine girmeyen durum geçişleri,
/// stok yetersizliği, geçersiz kupon kullanımı vb.
/// HTTP 400 (Bad Request) ile eşleştirilir.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public string? RuleCode { get; }

    public BusinessRuleException(string message, string? ruleCode = null)
        : base(message)
    {
        RuleCode = ruleCode;
    }
}
