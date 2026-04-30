using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Orders;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetOrderById;

/// <summary>
/// <see cref="GetOrderByIdQuery"/> için MediatR işleyicisi. Siparişi depodan yükler, kullanıcı sahipliğini doğrular
/// ve AutoMapper ile <see cref="OrderDto"/> üretir; kalıcı durum değiştirmez.
/// </summary>
/// <param name="orders">Sipariş okuma için depo.</param>
/// <param name="mapper">Domain <see cref="Order"/> varlığından sözleşme DTO’suna dönüşüm.</param>
public sealed class GetOrderByIdQueryHandler(
    IOrderRepository orders,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    /// <summary>
    /// Sorgu adımlarını uygular: siparişi getir, sahiplik kontrolü, DTO haritalama.
    /// </summary>
    /// <param name="request">Sipariş ve kullanıcı kimliklerini içerir.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Yetkili kullanıcı için siparişin <see cref="OrderDto"/> görünümünü döndürür.</returns>
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Adım 1: İstenen siparişi kimliğe göre yükle; yoksa bulunamadı istisnası (kaynak sızıntısı olmaması için tutarlı mesaj).
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        // Adım 2: İsteği yapan kullanıcının sipariş sahibi olduğunu doğrula; değilse yetkisiz erişim istisnası.
        if (order.UserId != request.UserId)
            throw new UnauthorizedException("You are not allowed to access this order.");

        // Adım 3: Varlığı API sözleşmesi DTO’suna haritala ve çağırana döndür.
        return mapper.Map<OrderDto>(order);
    }
}
