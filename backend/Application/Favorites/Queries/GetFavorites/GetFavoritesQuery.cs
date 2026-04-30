using Gulora.Contracts.Favorites;
using MediatR;

namespace Gulora.Application.Favorites.Queries.GetFavorites;

/// <summary>
/// Belirli bir kullanıcının favori listesindeki ürünleri, ürün ve kategori bilgisiyle birlikte döndüren sorgu.
/// </summary>
/// <remarks>
/// İşleyici silinmiş ürünlere ait favori satırlarını yanıttan düşürür; böylece istemci tutarlı bir ürün listesi görür.
/// </remarks>
/// <param name="UserId">Favori kayıtları getirilecek kullanıcının benzersiz tanımlayıcısı.</param>
public sealed record GetFavoritesQuery(Guid UserId) : IRequest<IReadOnlyList<FavoriteProductDto>>;
