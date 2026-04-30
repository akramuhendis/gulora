using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Son kullanıcının kendi siparişini iptal etme talebini temsil eden komut. Başarılı tamamlanmada
/// MediatR <see cref="Unit"/> döner; gerçek durum geçişi ve iş kuralları domain varlığı ve işleyicide uygulanır.
/// </summary>
/// <remarks>
/// İptal yalnızca sipariş sahibi tarafından ve domain’in izin verdiği durumlarda mümkündür; aksi halde
/// yetkisiz veya domain istisnası oluşur.
/// </remarks>
/// <param name="OrderId">İptal edilmek istenen siparişin benzersiz kimliği.</param>
/// <param name="UserId">
/// İşlemi başlatan kullanıcının kimliği; yüklenen siparişin <c>UserId</c> alanı ile eşleşmezse erişim reddedilir.
/// </param>
public sealed record CancelOrderCommand(Guid OrderId, Guid UserId) : IRequest<Unit>, ICommand;
