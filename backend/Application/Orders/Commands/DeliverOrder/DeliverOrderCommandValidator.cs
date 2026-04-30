using FluentValidation;

namespace Gulora.Application.Orders.Commands.DeliverOrder;

/// <summary>
/// <see cref="DeliverOrderCommand"/> için FluentValidation doğrulayıcısı; yalnızca sipariş kimliği şekil denetimi yapar.
/// </summary>
public sealed class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
{
    /// <summary>
    /// Sipariş kimliğinin boş GUID olmamasını zorunlu kılar.
    /// </summary>
    public DeliverOrderCommandValidator()
    {
        // Hedef sipariş belirtilmeden teslim komutu çalıştırılamaz.
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
