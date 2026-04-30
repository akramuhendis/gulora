using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Orders;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetOrders;

/// <summary>
/// <see cref="GetOrdersQuery"/> için MediatR işleyicisi. Kullanıcı kimliğine göre siparişleri depodan alır
/// ve her birini <see cref="OrderDto"/> listesine haritalar.
/// </summary>
/// <param name="orders">Kullanıcı bazlı sipariş listeleme için depo.</param>
/// <param name="mapper">Koleksiyon veya öğe bazlı varlıktan DTO dönüşümü için AutoMapper örneği.</param>
public sealed class GetOrdersQueryHandler(
    IOrderRepository orders,
    IMapper mapper) : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    /// <summary>
    /// Listeleme adımlarını uygular: kullanıcıya göre siparişleri getir, DTO’ya dönüştür.
    /// </summary>
    /// <param name="request">Siparişleri listelenecek kullanıcı kimliğini taşır.</param>
    /// <param name="cancellationToken">İş iptali için iptal belirteci.</param>
    /// <returns>Kullanıcıya ait siparişlerin salt okunur DTO listesini döndürür (boş olabilir).</returns>
    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Adım 1: Veri kaynağından ilgili kullanıcının siparişlerini asenkron olarak al.
        var result = await orders.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        // Adım 2: Domain veya kalıcılık modelini istemci sözleşmesi listesine haritala ve döndür.
        return mapper.Map<IReadOnlyList<OrderDto>>(result);
    }
}
