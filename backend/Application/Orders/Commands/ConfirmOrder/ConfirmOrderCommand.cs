using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.ConfirmOrder;

/// <summary>
/// Yönetim veya arka ofis süreçlerinde siparişin onaylandığı (ör. ödeme onayı veya iç operasyon onayı)
/// duruma geçirilmesini isteyen komut. Yanıt olarak <see cref="Unit"/> döner; asıl durum geçişi domain’de yapılır.
/// </summary>
/// <remarks>
/// Genellikle müşteri oluşturma komutundan sonra gelen yaşam döngüsü adımıdır; geçersiz durumda domain istisnası beklenir.
/// </remarks>
/// <param name="OrderId">Onaylanacak siparişin benzersiz kimliği.</param>
public sealed record ConfirmOrderCommand(Guid OrderId) : IRequest<Unit>, ICommand;
