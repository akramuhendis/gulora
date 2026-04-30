using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Favorites.Commands.AddFavorite;

/// <summary>
/// Kullanıcının bir ürünü favori listesine eklemesini temsil eden komut; başarılı yanıtta yeni favori kaydının kimliği döner.
/// </summary>
/// <remarks>
/// İşleyici ürün varlığını doğrular, mevcut favorilerle birlikte alan kurallarını (ör. üst limit) <see cref="Gulora.Domain.Entities.Favorite"/> üzerinde uygular.
/// </remarks>
/// <param name="UserId">Favoriyi ekleyen kullanıcının benzersiz tanımlayıcısı.</param>
/// <param name="ProductId">Favorilere eklenecek ürünün benzersiz tanımlayıcısı.</param>
public sealed record AddFavoriteCommand(Guid UserId, Guid ProductId) : IRequest<Guid>, ICommand;
