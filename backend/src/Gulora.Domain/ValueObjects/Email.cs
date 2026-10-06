using System.Text.RegularExpressions;

namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Doğrulanmış e-posta adresi değer nesnesi.
/// Entity'ler ham string yerine bu türü kullanır — domain kuralı ihlalinde
/// <see cref="ArgumentException"/> fırlatılır, geçersiz durum oluşmaz.
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(trimmed))
            throw new ArgumentException($"'{value}' geçerli bir e-posta adresi değil.", nameof(value));

        return new Email(trimmed);
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
