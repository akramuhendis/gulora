namespace Gulora.Domain.Exceptions;

/// <summary>
/// Tüm domain-seviye iş kuralı ihlallerinin temel istisnası.
/// Bu istisnadan türeyen sınıflar iş kuralı hatalarını temsil eder —
/// altyapı veya teknik hatalar değil.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception inner) : base(message, inner) { }
}
