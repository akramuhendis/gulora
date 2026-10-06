namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Siparişin oluşturulduğu andaki adresin değişmez anlık görüntüsü.
/// Kullanıcı adresi sonradan değişse bile sipariş kaydı bozulmaz.
/// JSON olarak <see cref="Gulora.Domain.Entities.Order.AddressSnapshot"/> alanında saklanır.
/// </summary>
public sealed record AddressSnapshot(
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode = null,
    string? CountryCode = "AZ"
)
{
    /// <summary>Temel doğrulama — boş alan kontrolü.</summary>
    public static AddressSnapshot Create(
        string recipientName,
        string phoneNumber,
        string city,
        string district,
        string fullLine,
        string? postalCode = null,
        string? countryCode = "AZ")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullLine);

        return new AddressSnapshot(recipientName, phoneNumber, city, district, fullLine, postalCode, countryCode);
    }

    /// <summary>Görüntüleme için kısa tek-satır formatlı adres.</summary>
    public string ToDisplayString() => $"{RecipientName} — {FullLine}, {District}, {City}";
}
