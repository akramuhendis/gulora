using MediatR;

namespace Gulora.Application.Users.Commands.DeleteAddress;

/// <summary>Kullanıcının teslimat adresini siler (fiziksel silme).</summary>
public sealed record DeleteAddressCommand(Guid UserId, Guid AddressId) : IRequest;
