using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Favorites;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Favorites.Queries.GetFavorites;

/// <summary>
/// Kullanıcıya ait favori satırlarını yükler; ürünleri toplu getirir, silinmiş ürünleri eler, kategori bilgisini tamamlar.
/// </summary>
/// <remarks>
/// Ürün başına tekrarlayan kategori sorgusu önlenmesi için tüm kategoriler tek seferde yüklenir ve kimlik sözlüğü ile eşlenir.
/// </remarks>
public sealed class GetFavoritesQueryHandler(
    IFavoriteRepository favorites,
    IProductRepository products,
    ICategoryRepository categories,
    IMapper mapper) : IRequestHandler<GetFavoritesQuery, IReadOnlyList<FavoriteProductDto>>
{
    /// <summary>
    /// Favorileri ve ilişkili ürünleri birleştirir; geçerli ürünler için <see cref="FavoriteProductDto"/> listesi üretir.
    /// </summary>
    /// <param name="request">Kullanıcı kimliği.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Favori kimliği, eklenme zamanı ve ürün DTO'su içeren liste.</returns>
    public async Task<IReadOnlyList<FavoriteProductDto>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
    {
        var favs = await favorites.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (favs.Count == 0)
            return []; // Erken çıkış: ürün ve kategori sorgusu yapılmaz

        // Favori satırlarındaki ürün kimlikleriyle toplu ürün yükleme (N+1 önleme)
        var productIds = favs.Select(f => f.ProductId).ToList();
        var loadedProducts = await products.GetByIdsAsync(productIds, cancellationToken).ConfigureAwait(false);
        var productById = loadedProducts.ToDictionary(p => p.Id);

        var allCategories = await categories.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var catById = allCategories.ToDictionary(c => c.Id);

        var dtos = new List<FavoriteProductDto>(favs.Count);
        foreach (var fav in favs)
        {
            // Ürün artık yoksa (silinmiş vb.) bu favori satırını yanıta dahil etme
            if (!productById.TryGetValue(fav.ProductId, out var product))
                continue;

            var productDto = mapper.Map<ProductDto>(product);
            if (catById.TryGetValue(product.CategoryId, out var cat))
                productDto = productDto with { CategorySlug = cat.Slug, CategoryName = cat.Name };
            else
                productDto = productDto with { CategorySlug = string.Empty, CategoryName = string.Empty };

            dtos.Add(new FavoriteProductDto(fav.Id, fav.CreatedAt, productDto));
        }

        return dtos;
    }
}
