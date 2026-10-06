namespace Gulora.Contracts.Users;

/// <summary>PUT api/v1/users/me/addresses/{id} gövdesi.</summary>
public sealed record UpdateAddressRequest(
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode = null);
