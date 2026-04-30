using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Addresses.Commands.DeleteAddress;

/// <summary>
/// Kullanıcıya ait bir adresi kalıcı olarak siler; sahiplik ve varlık kontrollerini uygular.
/// </summary>
public sealed class DeleteAddressCommandHandler(
    IAddressRepository addresses) : IRequestHandler<DeleteAddressCommand, Unit>
{
    /// <summary>
    /// Adresi yükler, kullanıcıya ait olduğunu doğrular ve depodan kaldırır.
    /// </summary>
    /// <param name="request">Silinecek adres ve işlemi yapan kullanıcı kimliği.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>MediatR birim değeri.</returns>
    public async Task<Unit> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        // Adresi kimliğe göre getir; yoksa 404 benzeri hata fırlat.
        var address = await addresses.GetByIdAsync(request.AddressId, cancellationToken).ConfigureAwait(false);
        if (address is null)
            throw new NotFoundException("Address", request.AddressId);

        // Başka kullanıcının adresine erişimi engelle.
        if (address.UserId != request.UserId)
            throw new UnauthorizedException("You are not allowed to access this address.");

        // Sil ve değişiklikleri kalıcı hale getir.
        await addresses.RemoveAsync(address, cancellationToken).ConfigureAwait(false);
        await addresses.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
