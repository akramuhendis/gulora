using FluentValidation;

namespace Gulora.Application.Orders.Commands.StartPreparingOrder;

/// <summary>
/// <see cref="StartPreparingOrderCommand"/> için FluentValidation doğrulayıcısı; sipariş kimliği şekil denetimi yapar.
/// </summary>
public sealed class StartPreparingOrderCommandValidator : AbstractValidator<StartPreparingOrderCommand>
{
    /// <summary>
    /// Sipariş kimliğinin dolu olmasını zorunlu kılar.
    /// </summary>
    public StartPreparingOrderCommandValidator()
    {
        // Hazırlık komutu hedefsiz çalıştırılamaz.
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
