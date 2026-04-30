using Gulora.Domain.Common;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Kullanıcının “favoriler” listesindeki tek bir ürün ilişkisini temsil eder.
/// Ürün detayı ayrıca <see cref="Product"/> üzerinden yüklenir; burada yalnızca kimlik çifti tutulur.
/// </summary>
public class Favorite : BaseEntity
{
    /// <summary>Favoriyi ekleyen kullanıcı.</summary>
    public Guid UserId { get; private set; }
    /// <summary>Favorilenen ürün.</summary>
    public Guid ProductId { get; private set; }

    private Favorite() { }

    /// <summary>
    /// Yeni favori ilişkisi. <paramref name="existingFavoriteProductIdsForUser"/> verilərsə,
    /// eyni məhsul təkrarı domain səviyyəsində rədd edilir (repository-dən yüklənmiş ID siyahısı).
    /// </summary>
    public static Favorite Create(Guid userId, Guid productId, IReadOnlyCollection<Guid>? existingFavoriteProductIdsForUser = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        if (existingFavoriteProductIdsForUser is not null && existingFavoriteProductIdsForUser.Contains(productId))
            throw new DomainException("Bu məhsul artıq sevimlilər siyahısındadır.");

        var favorite = new Favorite { UserId = userId, ProductId = productId };
        favorite.RegisterDomainEvent(new FavoriteAddedDomainEvent(favorite.Id, userId, productId));
        return favorite;
    }
}
