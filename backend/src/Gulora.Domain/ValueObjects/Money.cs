namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Para tutarı ve para birimi değer nesnesi.
/// İki <see cref="Money"/> yalnızca aynı para birimindeyken toplanabilir.
/// Aritmetik işlemler yeni Money örneği döndürür (immutable).
/// </summary>
public sealed record Money(decimal Amount, string Currency = "AZN")
{
    public static readonly Money Zero = new(0m, "AZN");

    public static Money Of(decimal amount, string currency = "AZN")
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Para tutarı negatif olamaz.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Para birimi boş olamaz.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return this with { Amount = Amount + other.Amount };
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Çıkarma işlemi negatif bakiyeye yol açamaz.");

        return this with { Amount = result };
    }

    public Money Multiply(decimal factor) =>
        this with { Amount = Math.Round(Amount * factor, 2, MidpointRounding.AwayFromZero) };

    public Money ApplyPercent(decimal percent)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "Yüzde 0-100 aralığında olmalıdır.");

        return Multiply(percent / 100m);
    }

    public bool IsZero => Amount == 0m;

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Para birimi uyuşmazlığı: {Currency} ile {other.Currency} toplanamaz.");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
