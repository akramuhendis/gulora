using MediatR;

namespace Gulora.Application.Users.Commands.AddAddress;

/// <summary>Kullanıcıya yeni teslimat adresi ekler. Başarıda adres Guid'i döner.</summary>
public sealed record AddAddressCommand(
    Guid UserId,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode = null,
    string CountryCode = "AZ",
    bool IsDefault = false
) : IRequest<Guid>;
