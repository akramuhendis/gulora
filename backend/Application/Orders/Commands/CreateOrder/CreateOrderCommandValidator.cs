using FluentValidation;
using Gulora.Contracts.Orders;
using Gulora.Domain.Enums;

namespace Gulora.Application.Orders.Commands.CreateOrder;

/// <summary>
/// <see cref="CreateOrderCommand"/> için FluentValidation tabanlı giriş doğrulaması. İş mantığından önce
/// MediatR pipeline’ında (veya eşdeğer tetikleyicide) çalışarak tutarsız isteklerin erken reddedilmesini sağlar.
/// </summary>
/// <remarks>
/// Ağır iş kuralları (stok, kupon geçerliliği, ürün aktifliği) işleyicide kalır; burada yalnızca şekil ve
/// zorunlu alan tutarlılığı denetlenir.
/// </remarks>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// Tüm kuralları kurucuda tanımlar: kullanıcı, sepet, adres ve planlı teslimat koşullu tarih kısıtları.
    /// </summary>
    public CreateOrderCommandValidator()
    {
        // Sipariş sahibi: boş GUID kabul edilmez.
        RuleFor(x => x.UserId).NotEmpty();

        // Sepet: en az bir kalem zorunludur; boş liste iş mantığına gitmeden reddedilir.
        RuleFor(x => x.Items).NotEmpty();

        // Aynı ürün kimliği birden fazla satırda tekrarlanamaz; birleştirme istemci veya API katmanında yapılmalıdır.
        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage(CreateOrderValidationMessages.DuplicateProductLines);

        // Her kalem için ürün kimliği ve pozitif miktar şartı.
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });

        // Teslimat adresi ve zorunlu alt alanlar: null veya boş metin kabul edilmez.
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address.RecipientName).NotEmpty();
        RuleFor(x => x.Address.RecipientPhone).NotEmpty();
        RuleFor(x => x.Address.City).NotEmpty();
        RuleFor(x => x.Address.District).NotEmpty();
        RuleFor(x => x.Address.Line).NotEmpty();

        // Planlı teslimat seçildiğinde tarih/saat bilgisi zorunludur; diğer teslimat türlerinde bu kural uygulanmaz.
        RuleFor(x => x.ScheduledDate).NotEmpty()
            .When(x => x.DeliveryType == DeliveryType.Scheduled);
    }
}
