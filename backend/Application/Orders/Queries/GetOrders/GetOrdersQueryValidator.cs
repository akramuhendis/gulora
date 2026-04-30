using FluentValidation;

namespace Gulora.Application.Orders.Queries.GetOrders;

/// <summary>
/// <see cref="GetOrdersQuery"/> için FluentValidation doğrulayıcısı. Listeleme öncesi kullanıcı kimliğinin
/// geçerli olduğunu doğrular.
/// </summary>
public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    /// <summary>
    /// Kullanıcı kimliği için boş olmama kuralını ekler.
    /// </summary>
    public GetOrdersQueryValidator()
    {
        // Hangi kullanıcının siparişleri listelenecekse kimlik zorunludur.
        RuleFor(x => x.UserId).NotEmpty();
    }
}
