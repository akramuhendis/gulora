namespace Gulora.Domain.Exceptions;

/// <summary>
/// İstemci aynı kaynağa çok sık istek attığında (ör. OTP yeniden gönderim).
/// HTTP 429 ile eşlenir.
/// </summary>
public sealed class TooManyRequestsException : DomainException
{
    public TooManyRequestsException(string message) : base(message) { }
}
