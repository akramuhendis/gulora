using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Commands.RequestReturn;

/// <summary>
/// <see cref="RequestReturnCommand"/> için MediatR işleyicisi. Siparişi yükler, kullanıcı sahipliğini doğrular,
/// domain üzerinden iade talebini kayda geçirir ve birim işte kalıcı yazar.
/// </summary>
/// <param name="orders">Sipariş okuma ve güncelleme için depo.</param>
public sealed class RequestReturnCommandHandler(
    IOrderRepository orders) : IRequestHandler<RequestReturnCommand, Unit>
{
    /// <summary>
    /// İade talebi adımlarını uygular: yükleme, yetki, domain talebi, kalıcılık.
    /// </summary>
    /// <param name="request">Sipariş, kullanıcı ve gerekçe bilgisini taşır.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Başarıda <see cref="Unit.Value"/> döner.</returns>
    public async Task<Unit> Handle(RequestReturnCommand request, CancellationToken cancellationToken)
    {
        // Adım 1: İade talebi yapılacak siparişi kimliğe göre yükle; kayıt yoksa bulunamadı istisnası.
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: Güvenlik: yalnızca sipariş sahibi iade talebinde bulunabilir.
        if (order.UserId != request.UserId)
            throw new UnauthorizedException("You are not allowed to access this order.");

        // Adım 3: Domain’e gerekçe ile birlikte iade talebini ilet ve değişiklikleri kalıcı depoya yaz.
        order.RequestReturn(request.Reason);
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
