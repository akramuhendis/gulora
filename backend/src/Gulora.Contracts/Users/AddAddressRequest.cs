namespace Gulora.Contracts.Users;

/// <summary>POST api/v1/users/me/addresses gövdesi.</summary>
public sealed record AddAddressRequest(
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode = null,
    string CountryCode = "AZ",
    bool IsDefault = false);
