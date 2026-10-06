using System.Text.RegularExpressions;

namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Azerbaycan ve uluslararası formatı destekleyen telefon numarası değer nesnesi.
/// Yalnızca rakam ve isteğe bağlı başlangıç '+' kabul edilir.
/// </summary>
public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex =
        new(@"^\+?[1-9]\d{6,14}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().Replace(" ", "").Replace("-", "");

        if (!PhoneRegex.IsMatch(normalized))
            throw new ArgumentException($"'{value}' geçerli bir telefon numarası değil.", nameof(value));

        return new PhoneNumber(normalized);
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    public override string ToString() => Value;
}
