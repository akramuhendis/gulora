using Gulora.Contracts.Addresses;
using MediatR;

namespace Gulora.Application.Addresses.Queries.GetAddresses;

/// <summary>
/// Belirtilen kullanıcıya ait tüm adresleri listelemek için sorgu.
/// </summary>
/// <param name="UserId">Adresleri getirilecek kullanıcının kimliği.</param>
public sealed record GetAddressesQuery(Guid UserId) : IRequest<IReadOnlyList<AddressDto>>;
