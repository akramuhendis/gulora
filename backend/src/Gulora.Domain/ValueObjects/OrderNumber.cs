namespace Gulora.Domain.ValueObjects;

/// <summary>
/// İnsan okunabilir, benzersiz sipariş numarası değer nesnesi.
/// Format: "GUL-{YYYYMMDD}-{sequence:D4}" — örn. "GUL-20260502-0001".
/// Üretim mantığı Application katmanında (OrderNumberGenerator servisinde) barındırılır;
/// bu nesne yalnızca saklama ve doğrulama sorumluluğunu taşır.
/// </summary>
public sealed record OrderNumber
{
    public string Value { get; }

    private OrderNumber(string value) => Value = value;

    public static OrderNumber Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!value.StartsWith("GUL-", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Geçersiz sipariş numarası formatı.", nameof(value));

        return new OrderNumber(value.ToUpperInvariant());
    }

    public static OrderNumber Generate(DateOnly date, int sequence) =>
        new($"GUL-{date:yyyyMMdd}-{sequence:D4}");

    public static implicit operator string(OrderNumber number) => number.Value;

    public override string ToString() => Value;
}
