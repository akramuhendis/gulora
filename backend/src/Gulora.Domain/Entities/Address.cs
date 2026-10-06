using Gulora.Domain.Common;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcıya ait kayıtlı teslimat adresi.
/// User aggregate'inin alt entity'sidir.
/// Fiziksel silme desteklenir (kullanıcı adresi silebilir).
/// </summary>
public sealed class Address : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public string FullLine { get; private set; } = string.Empty;
    public string? PostalCode { get; private set; }
    public string CountryCode { get; private set; } = "AZ";
    public bool IsDefault { get; private set; }

    private Address() { }

    public static Address Create(
        Guid userId,
        string title,
        string recipientName,
        string phoneNumber,
        string city,
        string district,
        string fullLine,
        string? postalCode = null,
        string countryCode = "AZ",
        bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullLine);

        return new Address
        {
            UserId = userId,
            Title = title.Trim(),
            RecipientName = recipientName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            City = city.Trim(),
            District = district.Trim(),
            FullLine = fullLine.Trim(),
            PostalCode = postalCode?.Trim(),
            CountryCode = countryCode.ToUpperInvariant(),
            IsDefault = isDefault
        };
    }

    public void Update(
        string title,
        string recipientName,
        string phoneNumber,
        string city,
        string district,
        string fullLine,
        string? postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(district);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullLine);

        Title = title.Trim();
        RecipientName = recipientName.Trim();
        PhoneNumber = phoneNumber.Trim();
        City = city.Trim();
        District = district.Trim();
        FullLine = fullLine.Trim();
        PostalCode = postalCode?.Trim();
        MarkUpdated();
    }

    public void SetAsDefault() => IsDefault = true;

    public void ClearDefault() => IsDefault = false;
}
