using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.StartPreparingOrder;

/// <summary>
/// <see cref="StartPreparingOrderCommand"/> için MediatR işleyicisi. Siparişi yükler, hazırlık başlangıcı
/// domain çağrısını yapar ve kalıcı kaydeder.
/// </summary>
/// <param name="orders">Sipariş okuma ve güncelleme için depo.</param>
public sealed class StartPreparingOrderCommandHandler(
    IOrderRepository orders) : IRequestHandler<StartPreparingOrderCommand, Unit>
{
    /// <summary>
    /// Hazırlık başlatma adımlarını uygular: siparişi getir, domain geçişi, kalıcılık.
    /// </summary>
    /// <param name="request">Hedef sipariş kimliğini içerir.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(StartPreparingOrderCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: Hazırlık durumuna alınacak siparişi kimliğe göre oku; bulunamazsa istisna.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Domain’de hazırlık başlatma kuralını uygula ve birim iş ile değişiklikleri kalıcı yap.
        order.StartPreparing();
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
