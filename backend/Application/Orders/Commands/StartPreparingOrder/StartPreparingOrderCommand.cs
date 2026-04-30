using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.StartPreparingOrder;

/// <summary>
/// Siparişin hazırlanmaya başlandığını (ör. paketleme, mutfak veya depo toplama) belirten komut.
/// Onay sonrası lojistik öncesi operasyonel adım olarak kullanılır; yanıt <see cref="Unit"/> şeklindedir.
/// </summary>
/// <remarks>
/// Geçerli durum geçişleri <see cref="Gulora.Domain.Entities.Order"/> üzerindeki <c>StartPreparing</c> metodu ile kısıtlanır.
/// </remarks>
/// <param name="OrderId">Hazırlığa alınacak siparişin benzersiz kimliği.</param>
public sealed record StartPreparingOrderCommand(Guid OrderId) : IRequest<Unit>, ICommand;
