using FluentValidation;

namespace Gulora.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Teslimat adresi seçilmelidir.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş en az bir kalem içermelidir.");

        RuleFor(x => x.DeliveryType)
            .IsInEnum().WithMessage("Geçersiz teslimat türü.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Geçersiz ödeme yöntemi.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Ürün kimliği boş olamaz.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Ürün miktarı en az 1 olmalıdır.")
                .LessThanOrEqualTo(100).WithMessage("Ürün miktarı en fazla 100 olabilir.");
        });

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Kupon kodu en fazla 50 karakter olabilir.")
            .When(x => x.CouponCode is not null);

        RuleFor(x => x.GiftCardMessage)
            .MaximumLength(2000).WithMessage("Kart mesajı en fazla 2000 karakter olabilir.")
            .When(x => x.GiftCardMessage is not null);

        RuleFor(x => x.GiftCardSenderName)
            .MaximumLength(120).WithMessage("Göndərən adı en fazla 120 karakter olabilir.")
            .When(x => x.GiftCardSenderName is not null);

        RuleFor(x => x.GiftCardOccasion)
            .MaximumLength(80).WithMessage("Məqsəd sahəsi en fazla 80 karakter olabilir.")
            .When(x => x.GiftCardOccasion is not null);
    }
}
