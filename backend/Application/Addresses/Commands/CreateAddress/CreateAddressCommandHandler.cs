using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Addresses;
using Gulora.Domain.Entities;
using MediatR;

namespace Gulora.Application.Addresses.Commands.CreateAddress;

/// <summary>
/// Yeni teslimat adresi oluşturur; varsayılan adres seçildiyse diğer varsayılanları kaldırır ve DTO döner.
/// </summary>
public sealed class CreateAddressCommandHandler(
    IAddressRepository addresses,
    IMapper mapper) : IRequestHandler<CreateAddressCommand, AddressDto>
{
    /// <summary>
    /// Varsayılan adres tutarlılığını sağlar, alan <see cref="Address"/> oluşturur, kaydeder ve eşlemeli DTO üretir.
    /// </summary>
    /// <param name="request">Adres alanları ve kullanıcı kimliği.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Oluşturulan adresin sözleşme DTO'su.</returns>
    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        // Yeni kayıt varsayılan olacaksa, kullanıcının mevcut varsayılan adreslerini sıfırla (tek varsayılan).
        if (request.IsDefault)
        {
            var existing = await addresses.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
            foreach (var addr in existing.Where(a => a.IsDefault))
                addr.UnsetDefault();
        }

        var address = Address.Create(
            request.UserId,
            request.Title,
            request.RecipientName,
            request.RecipientPhone,
            request.City,
            request.District,
            request.Line,
            request.PostalCode,
            request.IsDefault);

        await addresses.AddAsync(address, cancellationToken).ConfigureAwait(false);
        await addresses.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<AddressDto>(address);
    }
}
