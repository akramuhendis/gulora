using Gulora.Domain.Enums;
using MediatR;

namespace Gulora.Application.Orders.Commands.CreateOrder;

/// <summary>
/// Yeni sipariş oluşturur.
/// Stok azaltma, adres doğrulaması ve opsiyonel kupon uygulaması handler'da yapılır.
/// Başarı durumunda oluşturulan siparişin Guid'i döner.
/// </summary>
public sealed record CreateOrderCommand(
    Guid UserId,
    Guid AddressId,
    DeliveryType DeliveryType,
    PaymentMethod PaymentMethod,
    List<OrderItemInput> Items,
    string? CouponCode = null,
    string? GiftCardMessage = null,
    string? GiftCardSenderName = null,
    string? GiftCardOccasion = null
) : IRequest<Guid>;

public sealed record OrderItemInput(Guid ProductId, int Quantity);
