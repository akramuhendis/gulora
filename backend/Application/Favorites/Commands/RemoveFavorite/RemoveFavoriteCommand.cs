using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Kullanıcının belirli bir ürünü favori listesinden kaldırmasını temsil eden komut; MediatR <see cref="Unit"/> ile tamamlanır.
/// </summary>
/// <remarks>
/// Kayıt kullanıcı–ürün çiftine göre bulunur; yoksa işleyici <see cref="Gulora.Domain.Exceptions.NotFoundException"/> fırlatır.
/// </remarks>
/// <param name="UserId">İşlemi yapan kullanıcının benzersiz tanımlayıcısı.</param>
/// <param name="ProductId">Favorilerden çıkarılacak ürünün benzersiz tanımlayıcısı.</param>
public sealed record RemoveFavoriteCommand(Guid UserId, Guid ProductId) : IRequest<Unit>, ICommand;
