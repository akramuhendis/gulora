using FluentValidation;

namespace Gulora.Application.Orders.Queries.GetOrderById;

/// <summary>
/// <see cref="GetOrderByIdQuery"/> için FluentValidation doğrulayıcısı. Sorgu çalışmadan önce kimlik alanlarının
/// dolu olduğundan emin olur.
/// </summary>
public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    /// <summary>
    /// Sipariş ve kullanıcı kimliği için zorunluluk kurallarını ekler.
    /// </summary>
    public GetOrderByIdQueryValidator()
    {
        // Hangi siparişin getirileceği belirtilmelidir.
        RuleFor(x => x.OrderId).NotEmpty();
        // Sahiplik kontrolü için kullanıcı kimliği zorunludur.
        RuleFor(x => x.UserId).NotEmpty();
    }
}
