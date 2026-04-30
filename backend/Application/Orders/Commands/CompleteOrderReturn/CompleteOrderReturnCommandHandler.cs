using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.CompleteOrderReturn;

/// <summary>
/// <see cref="CompleteOrderReturnCommand"/> için MediatR işleyicisi. Siparişi yükler, iade tamamlama
/// domain işlemini uygular ve kalıcı yazar.
/// </summary>
/// <param name="orders">Sipariş deposu (okuma ve güncelleme).</param>
public sealed class CompleteOrderReturnCommandHandler(
    IOrderRepository orders) : IRequestHandler<CompleteOrderReturnCommand, Unit>
{
    /// <summary>
    /// İade tamamlama adımlarını sırayla yürütür: yükleme, domain tamamlama, kayıt.
    /// </summary>
    /// <param name="request">Hedef sipariş kimliğini içerir.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(CompleteOrderReturnCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: İade süreci sonlandırılacak siparişi getir; yoksa bulunamadı.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Domain’de iade tamamlama geçişini uygula ve değişiklikleri veri deposuna işle.
        order.CompleteReturn();
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
