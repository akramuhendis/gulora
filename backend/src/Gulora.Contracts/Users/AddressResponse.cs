namespace Gulora.Contracts.Users;

/// <summary>Kullanıcıya ait kayıtlı teslimat adresi yanıtı.</summary>
public sealed record AddressResponse(
    Guid Id,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode,
    string CountryCode,
    bool IsDefault);
