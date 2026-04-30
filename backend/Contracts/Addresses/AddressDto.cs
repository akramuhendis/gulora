namespace Gulora.Contracts.Addresses;

public record AddressDto(
    Guid Id,
    string Title,
    string RecipientName,
    string RecipientPhone,
    string City,
    string District,
    string Line,
    string? PostalCode,
    bool IsDefault);
