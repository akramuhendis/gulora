using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Kullanıcı ve ürün çiftine göre favori kaydını bulur, siler ve standart MediatR birim sonucu döndürür.
/// </summary>
/// <remarks>
/// Silme işleminden sonra <see cref="IFavoriteRepository.SaveChangesAsync"/> ile birim iş kalıcılığı tamamlanır.
/// </remarks>
public sealed class RemoveFavoriteCommandHandler(
    IFavoriteRepository favorites) : IRequestHandler<RemoveFavoriteCommand, Unit>
{
    /// <summary>
    /// Favori varlığını yükler; bulunamazsa hata, bulunursa kaldırma ve <see cref="Unit.Value"/> döner.
    /// </summary>
    /// <param name="request">Kullanıcı ve ürün kimlikleri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Başarılı silme sonrası birim değer.</returns>
    /// <exception cref="NotFoundException">İlgili favori kaydı yoksa.</exception>
    public async Task<Unit> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favorite = await favorites.GetAsync(request.UserId, request.ProductId, cancellationToken).ConfigureAwait(false);
        if (favorite is null)
            throw new NotFoundException("Favorite", $"{request.UserId}/{request.ProductId}");

        await favorites.RemoveAsync(favorite, cancellationToken).ConfigureAwait(false);
        await favorites.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
