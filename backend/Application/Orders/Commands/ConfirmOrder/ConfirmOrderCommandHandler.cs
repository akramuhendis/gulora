using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.ConfirmOrder;

/// <summary>
/// <see cref="ConfirmOrderCommand"/> için MediatR işleyicisi. Siparişi yükler, domain onay metodunu çağırır
/// ve birim iş ile kalıcı kaydeder.
/// </summary>
/// <remarks>
/// Yetkilendirme bu işleyicide açıkça yapılmaz; gerekirse üst katman (API/policy) tarafından kısıtlanmalıdır.
/// </remarks>
/// <param name="orders">Sipariş okuma ve güncelleme için depo soyutlaması.</param>
public sealed class ConfirmOrderCommandHandler(
    IOrderRepository orders) : IRequestHandler<ConfirmOrderCommand, Unit>
{
    /// <summary>
    /// Onay adımlarını sırayla uygular: siparişi getir, domain onayı, kalıcılık.
    /// </summary>
    /// <param name="request">Onaylanacak sipariş kimliğini taşır.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: Siparişi veri kaynağından yükle; bulunamazsa bulunamadı istisnası fırlatılır.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Domain’de onay geçişini uygula (durum ve olası yan etkiler) ve değişiklikleri kalıcı yap.
        order.Confirm();
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
