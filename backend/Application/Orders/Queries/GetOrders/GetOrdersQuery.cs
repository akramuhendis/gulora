using Gulora.Contracts.Orders;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetOrders;

/// <summary>
/// Belirtilen kullanıcıya ait tüm siparişlerin listelenmesi için okuma sorgusu. Yanıt salt okunur
/// <see cref="OrderDto"/> koleksiyonudur; sıralama ve sayfalama politikası depo uygulamasına bağlıdır.
/// </summary>
/// <remarks>
/// Bu sorgu <see cref="IRequest{TResponse}"/> ile işaretlenir; komutlardan farklı olarak yan etki beklenmez.
/// </remarks>
/// <param name="UserId">Listelenecek siparişlerin sahibi olan kullanıcının benzersiz kimliği.</param>
public sealed record GetOrdersQuery(Guid UserId) : IRequest<IReadOnlyList<OrderDto>>;
