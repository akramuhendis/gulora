using Gulora.Domain.Exceptions;

namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Para tutarı ve para birimini birlikte taşıyan değer nesnesi; tutar və valyuta domen qaydaları ilə sıxıdır.
/// </summary>
public sealed record Money
{
    /// <summary>Sıfır və ya müsbət məbləğ.</summary>
    public decimal Amount { get; init; }

    /// <summary>Boş olmayan para birimi kodu (trim edilmiş).</summary>
    public string Currency { get; init; } = "AZN";

    public Money(decimal amount, string currency = "AZN")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        Amount = amount;
        Currency = currency.Trim();
    }

    /// <summary>Sıfır tutarlı, varsayılan para birimli para nesnesi.</summary>
    public static Money Zero => new(0);

    /// <summary>Aynı para biriminde iki tutarı toplar.</summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        try
        {
            return this with { Amount = checked(Amount + other.Amount) };
        }
        catch (OverflowException)
        {
            throw new DomainException("Məbləğ toplama zamanı həddi aşıb.");
        }
    }

    /// <summary>İkinci tutarı düşər; nəticə mənfi ola bilməz.</summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var next = Amount - other.Amount;
        if (next < 0)
            throw new DomainException("Məbləğ mənfi ola bilməz.");
        return this with { Amount = next };
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
            throw new DomainException($"Valyuta uyğun deyil: {Currency} və {other.Currency}.");
    }

    /// <summary>Adet ile çarpım; çarpan və nəticə mənfi ola bilməz.</summary>
    public Money Multiply(int factor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(factor);
        try
        {
            var next = checked(Amount * factor);
            return this with { Amount = next };
        }
        catch (OverflowException)
        {
            throw new DomainException("Məbləğ çarpma zamanı həddi aşıb.");
        }
    }

    /// <summary>Liste veya log çıktıları için okunabilir metin.</summary>
    public override string ToString() => $"{Amount:F2} {Currency}";
}
