using MediatR;

namespace Gulora.Application.Users.Commands.UpdateAddress;

/// <summary>Kullanıcının mevcut adresini günceller.</summary>
public sealed record UpdateAddressCommand(
    Guid UserId,
    Guid AddressId,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullLine,
    string? PostalCode
) : IRequest;
