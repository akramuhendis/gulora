using FluentValidation;

namespace Gulora.Application.Orders.Commands.CancelOrder;

/// <summary>
/// <see cref="CancelOrderCommand"/> için FluentValidation doğrulayıcısı. İş mantığı çalışmadan önce
/// kimlik alanlarının dolu olduğundan emin olur.
/// </summary>
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    /// <summary>
    /// Sipariş ve kullanıcı kimlikleri için <c>NotEmpty</c> kurallarını kaydeder.
    /// </summary>
    public CancelOrderCommandValidator()
    {
        // İptal hedefi: geçerli bir sipariş kimliği zorunludur.
        RuleFor(x => x.OrderId).NotEmpty();
        // İşlemi yapan kullanıcı: anonim veya boş kimlik ile iptal isteği kabul edilmez.
        RuleFor(x => x.UserId).NotEmpty();
    }
}
