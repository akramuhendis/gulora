using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.CancelOrder;

/// <summary>
/// <see cref="CancelOrderCommand"/> için MediatR işleyicisi. Siparişi depodan yükler, talep eden kullanıcı
/// ile sipariş sahibini eşleştirir ve domain üzerinden iptal geçişini uygular; değişiklikleri kalıcı yazar.
/// </summary>
/// <remarks>
/// Bulunamayan sipariş <see cref="NotFoundException"/>, sahip olmayan kullanıcı <see cref="UnauthorizedException"/>
/// ile sonuçlanır; geçersiz durum geçişleri domain katmanında ele alınır.
/// </remarks>
/// <param name="orders">Sipariş okuma, güncelleme ve birim iş kaydı için sipariş deposu.</param>
public sealed class CancelOrderCommandHandler(
    IOrderRepository orders) : IRequestHandler<CancelOrderCommand, Unit>
{
    /// <summary>
    /// Siparişi iptal etmek için sıralı adımları yürütür: yükleme, sahiplik, domain iptali, kalıcılık.
    /// </summary>
    /// <param name="request">İptal edilecek sipariş ve işlemi yapan kullanıcı kimliklerini içerir.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda anlamsız <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: Siparişi kimliğe göre getir; kayıt yoksa standart bulunamadı istisnası ile işlem sonlanır.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Güvenlik: yalnızca sipariş sahibi iptal talebinde bulunabilir.
        if (order.UserId != request.UserId)
            throw new UnauthorizedException("You are not allowed to access this order.");

        // Adım 3: Domain iptal kuralını uygula (durum geçişi, olası yan etkiler) ve değişiklikleri veritabanına yaz.
        order.Cancel();
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
