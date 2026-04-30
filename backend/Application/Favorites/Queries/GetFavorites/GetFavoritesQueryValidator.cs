using FluentValidation;

namespace Gulora.Application.Favorites.Queries.GetFavorites;

/// <summary>
/// <see cref="GetFavoritesQuery"/> için kullanıcı kimliğinin geçerli (boş olmayan GUID) olmasını sağlar.
/// </summary>
/// <remarks>
/// Yetkilendirme genelde API katmanında yapılır; burada yalnızca taşıma doğruluğu kontrol edilir.
/// </remarks>
public sealed class GetFavoritesQueryValidator : AbstractValidator<GetFavoritesQuery>
{
    /// <summary>
    /// <see cref="GetFavoritesQuery.UserId"/> için NotEmpty kuralını ekler.
    /// </summary>
    public GetFavoritesQueryValidator()
    {
        // Varsayılan Guid ile tüm kullanıcıların favorilerini çekmeyi önle
        RuleFor(x => x.UserId).NotEmpty();
    }
}
