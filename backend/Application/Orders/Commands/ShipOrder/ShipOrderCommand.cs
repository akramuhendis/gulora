using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.ShipOrder;

/// <summary>
/// Siparişin kargoya verildi veya yola çıktı olarak işaretlenmesini isteyen komut. Lojistik/fulfillment
/// sürecinin bir parçasıdır; başarılı işlemde <see cref="Unit"/> döner.
/// </summary>
/// <remarks>
/// Durum sırası domain tarafından korunur; uygun olmayan önceki durumda domain istisnası oluşabilir.
/// </remarks>
/// <param name="OrderId">Sevk edilecek siparişin benzersiz kimliği.</param>
public sealed record ShipOrderCommand(Guid OrderId) : IRequest<Unit>, ICommand;
