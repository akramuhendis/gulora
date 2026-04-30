using Gulora.Contracts.Orders;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetOrderById;

/// <summary>
/// Tek bir siparişin ayrıntılarını, isteği yapan kullanıcıya ait olduğu doğrulandıktan sonra getirmek için okuma sorgusu.
/// Yanıt <see cref="OrderDto"/> olup salt okunur veri döndürür.
/// </summary>
/// <remarks>
/// Güvenlik: <paramref name="UserId"/> ile sipariş sahibi eşleştirilmezse işleyici yetkisiz istisnası fırlatır.
/// </remarks>
/// <param name="OrderId">Detayı istenen siparişin benzersiz kimliği.</param>
/// <param name="UserId">Erişim kontrolünde kullanılacak kullanıcı kimliği (sipariş sahibi ile karşılaştırılır).</param>
public sealed record GetOrderByIdQuery(Guid OrderId, Guid UserId) : IRequest<OrderDto>;
