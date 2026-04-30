using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.CompleteOrderReturn;

/// <summary>
/// İade sürecinin tamamlandığını (ör. para iadesi veya stok geri alımının kesinleşmesi) belirten yönetim komutu.
/// Müşteri talebinden sonra arka ofis onayı veya muhasebe adımı ile ilişkilendirilebilir.
/// </summary>
/// <remarks>
/// Geçerli önceki durum ve yan etkiler domain katmanında tanımlıdır; bu komut yalnızca niyeti taşır.
/// </remarks>
/// <param name="OrderId">İadesi tamamlanacak siparişin benzersiz kimliği.</param>
public sealed record CompleteOrderReturnCommand(Guid OrderId) : IRequest<Unit>, ICommand;
