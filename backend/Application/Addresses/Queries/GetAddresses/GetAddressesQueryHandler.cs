using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Addresses;
using MediatR;

namespace Gulora.Application.Addresses.Queries.GetAddresses;

/// <summary>
/// Kullanıcı kimliğine göre adres listesini depodan okur ve DTO koleksiyonuna eşler.
/// </summary>
public sealed class GetAddressesQueryHandler(
    IAddressRepository addresses,
    IMapper mapper) : IRequestHandler<GetAddressesQuery, IReadOnlyList<AddressDto>>
{
    /// <summary>
    /// Kullanıcının adreslerini veri kaynağından alır ve salt okunur DTO listesine dönüştürür.
    /// </summary>
    /// <param name="request">Sorgulanacak kullanıcı kimliği.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Adres DTO'larının salt okunur listesi.</returns>
    public async Task<IReadOnlyList<AddressDto>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var result = await addresses.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        return mapper.Map<IReadOnlyList<AddressDto>>(result);
    }
}
