using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.DeliverOrder;

/// <summary>
/// <see cref="DeliverOrderCommand"/> için MediatR işleyicisi. Siparişi yükler, domain teslim metodunu çağırır
/// ve değişiklikleri kalıcı depoya yazar.
/// </summary>
/// <param name="orders">Sipariş okuma ve güncelleme için sipariş deposu.</param>
public sealed class DeliverOrderCommandHandler(
    IOrderRepository orders) : IRequestHandler<DeliverOrderCommand, Unit>
{
    /// <summary>
    /// Teslim adımlarını yürütür: siparişi getir, teslim durumuna geçir, kaydet.
    /// </summary>
    /// <param name="request">Teslim edilecek sipariş kimliğini içerir.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(DeliverOrderCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: Teslimatı güncellenecek siparişi kimliğe göre yükle; yoksa bulunamadı istisnası.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Domain’de teslim edildi geçişini uygula ve tüm izleyen değişiklikleri veri deposuna işle.
        order.Deliver();
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
