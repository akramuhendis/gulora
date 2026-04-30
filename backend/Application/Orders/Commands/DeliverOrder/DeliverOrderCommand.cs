using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.DeliverOrder;

/// <summary>
/// Siparişin müşteriye teslim edildi olarak işaretlenmesini isteyen komut. Teslimat yaşam döngüsünde
/// genellikle sevk sonrası son adımlardan biridir; yanıt <see cref="Unit"/> olup kalıcı güncelleme işleyicide yapılır.
/// </summary>
/// <remarks>
/// Geçersiz durum geçişleri (ör. henüz sevk edilmemiş sipariş) domain tarafından engellenir.
/// </remarks>
/// <param name="OrderId">Teslimi tamamlanacak siparişin benzersiz kimliği.</param>
public sealed record DeliverOrderCommand(Guid OrderId) : IRequest<Unit>, ICommand;
