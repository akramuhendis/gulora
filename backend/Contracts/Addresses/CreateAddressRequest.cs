namespace Gulora.Contracts.Addresses;

public record CreateAddressRequest(
    string Title,
    string RecipientName,
    string RecipientPhone,
    string City,
    string District,
    string Line,
    string? PostalCode,
    bool IsDefault = false);
