using FluentValidation;

namespace Gulora.Application.Favorites.Commands.AddFavorite;

/// <summary>
/// <see cref="AddFavoriteCommand"/> için kullanıcı ve ürün GUID alanlarının dolu olması doğrulaması.
/// </summary>
/// <remarks>
/// Ürünün sistemde varlığı ve favori iş kuralları işleyicide kontrol edilir.
/// </remarks>
public sealed class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    /// <summary>
    /// Her iki tanımlayıcı için <c>NotEmpty</c> kuralını kaydeder.
    /// </summary>
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
