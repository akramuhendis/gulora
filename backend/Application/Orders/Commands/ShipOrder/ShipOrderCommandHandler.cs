using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.ShipOrder;

/// <summary>
/// <see cref="ShipOrderCommand"/> için MediatR işleyicisi. Siparişi yükler, sevk durumunu domain ile günceller
/// ve birim işte kalıcı yazar.
/// </summary>
/// <param name="orders">Sipariş erişimi ve kalıcılık için depo.</param>
public sealed class ShipOrderCommandHandler(
    IOrderRepository orders) : IRequestHandler<ShipOrderCommand, Unit>
{
    /// <summary>
    /// Sevk adımlarını sırayla uygular: yükleme, domain sevk çağrısı, kayıt.
    /// </summary>
    /// <param name="request">Sevk edilecek sipariş kimliğini taşır.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: Sevk durumu güncellenecek siparişi veri kaynağından getir.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Domain’de sevk edildi geçişini uygula ve değişiklikleri kalıcı depoya aktar.
        order.Ship();
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
