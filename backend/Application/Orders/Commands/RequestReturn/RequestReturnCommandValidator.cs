using FluentValidation;

namespace Gulora.Application.Orders.Commands.RequestReturn;

/// <summary>
/// <see cref="RequestReturnCommand"/> için FluentValidation doğrulayıcısı. Kimlikler ve gerekçe metninin
/// şekil ve makul uzunluk sınırları içinde olduğundan emin olur.
/// </summary>
public sealed class RequestReturnCommandValidator : AbstractValidator<RequestReturnCommand>
{
    /// <summary>
    /// Sipariş, kullanıcı ve gerekçe alanları için kuralları kaydeder.
    /// </summary>
    public RequestReturnCommandValidator()
    {
        // Hedef sipariş belirtilmelidir.
        RuleFor(x => x.OrderId).NotEmpty();
        // Talep sahibi kullanıcı kimliği zorunludur (sahiplik kontrolü işleyicide yapılır).
        RuleFor(x => x.UserId).NotEmpty();
        // Gerekçe boş bırakılamaz; aşırı uzun metinler depolama ve arayüz için üst sınırı aşınca doğrulama hatası verilir.
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
