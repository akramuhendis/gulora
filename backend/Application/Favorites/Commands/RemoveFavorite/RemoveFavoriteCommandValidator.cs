using FluentValidation;

namespace Gulora.Application.Favorites.Commands.RemoveFavorite;

/// <summary>
/// <see cref="RemoveFavoriteCommand"/> için kullanıcı ve ürün tanımlayıcılarının boş olmaması doğrulaması.
/// </summary>
/// <remarks>
/// Favori satırının gerçekten varlığı işleyicide kontrol edilir; burada yalnızca istek biçimi doğrulanır.
/// </remarks>
public sealed class RemoveFavoriteCommandValidator : AbstractValidator<RemoveFavoriteCommand>
{
    /// <summary>
    /// Kullanıcı ve ürün GUID alanları için kuralları ekler.
    /// </summary>
    public RemoveFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
