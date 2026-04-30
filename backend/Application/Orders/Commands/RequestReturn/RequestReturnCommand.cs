using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Orders.Commands.RequestReturn;

/// <summary>
/// Son kullanıcının belirli bir sipariş için iade veya geri gönderim talebinde bulunmasını temsil eden komut.
/// Gerekçe metni müşteri hizmetleri ve onay süreçleri için kalıcı olarak ilişkilendirilir.
/// </summary>
/// <remarks>
/// Yalnızca sipariş sahibi talepte bulunabilir; doğrulama işleyicide kullanıcı eşleştirmesi ile yapılır.
/// </remarks>
/// <param name="OrderId">İade talep edilen siparişin benzersiz kimliği.</param>
/// <param name="UserId">Talebi oluşturan kullanıcı; siparişin sahibi ile karşılaştırılır.</param>
/// <param name="Reason">İade veya geri gönderim gerekçesi; doğrulayıcıda uzunluk üst sınırı ile kısıtlanır.</param>
public sealed record RequestReturnCommand(
    Guid OrderId,
    Guid UserId,
    string Reason) : IRequest<Unit>, ICommand;
