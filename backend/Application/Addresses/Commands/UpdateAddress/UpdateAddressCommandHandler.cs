using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Addresses;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Addresses.Commands.UpdateAddress;

/// <summary>
/// Mevcut adresi günceller; sahiplik, varsayılan adres tutarlılığı ve alan güncellemelerini yönetir.
/// </summary>
public sealed class UpdateAddressCommandHandler(
    IAddressRepository addresses,
    IMapper mapper) : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    /// <summary>
    /// Adresi yükler ve sahiplik doğrular; varsayılan bayrağına göre diğer adresleri ayarlar; alanları günceller ve DTO döner.
    /// </summary>
    /// <param name="request">Güncellenecek adres kimliği, kullanıcı ve yeni alan değerleri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Güncellenmiş adresin sözleşme DTO'su.</returns>
    public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await addresses.GetByIdAsync(request.AddressId, cancellationToken).ConfigureAwait(false);
        if (address is null)
            throw new NotFoundException("Address", request.AddressId);

        if (address.UserId != request.UserId)
            throw new UnauthorizedException("You are not allowed to access this address.");

        // Bu adres varsayılan yapılacaksa, aynı kullanıcının diğer varsayılanlarını kaldır (güncellenen kayıt hariç).
        if (request.IsDefault)
        {
            var existing = await addresses.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
            foreach (var addr in existing.Where(a => a.Id != request.AddressId && a.IsDefault))
                addr.UnsetDefault();
        }

        address.Update(
            request.Title,
            request.RecipientName,
            request.RecipientPhone,
            request.City,
            request.District,
            request.Line,
            request.PostalCode);

        // İstekteki varsayılan bayrağına göre bu kaydın varsayılan durumunu ayarla.
        if (request.IsDefault)
            address.SetAsDefault();
        else
            address.UnsetDefault();

        await addresses.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<AddressDto>(address);
    }
}
