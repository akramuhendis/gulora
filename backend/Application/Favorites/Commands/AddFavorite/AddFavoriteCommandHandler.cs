using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Favorites.Commands.AddFavorite;

/// <summary>
/// Ürünün var olduğunu doğrular; mevcut favori ürün kimlikleriyle <see cref="Favorite"/> oluşturur ve kalıcı olarak kaydeder.
/// </summary>
/// <remarks>
/// <see cref="Favorite.Create(System.Guid, System.Guid, System.Collections.Generic.IReadOnlySet{System.Guid})"/> ile
/// tekrar ve limit gibi alan içi kurallar merkezileştirilir.
/// </remarks>
public sealed class AddFavoriteCommandHandler(
    IFavoriteRepository favorites,
    IProductRepository products) : IRequestHandler<AddFavoriteCommand, Guid>
{
    /// <summary>
    /// Ürünü yükler, kullanıcının mevcut favorilerini alır, yeni favori varlığı oluşturur ve kaydeder.
    /// </summary>
    /// <param name="request">Kullanıcı ve ürün kimlikleri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Oluşturulan favori kaydının benzersiz kimliği.</returns>
    /// <exception cref="NotFoundException">Ürün bulunamadığında.</exception>
    public async Task<Guid> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(request.ProductId, cancellationToken).ConfigureAwait(false);
        if (product is null)
            throw new NotFoundException("Product", request.ProductId);

        // Aynı kullanıcının mevcut favori ürün kimlikleri; varlık tekrarını ve limiti kontrol için
        var existing = await favorites.GetByUserIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        var existingIds = existing.Select(f => f.ProductId).ToHashSet();

        var favorite = Favorite.Create(request.UserId, request.ProductId, existingIds);
        await favorites.AddAsync(favorite, cancellationToken).ConfigureAwait(false);
        await favorites.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return favorite.Id;
    }
}
