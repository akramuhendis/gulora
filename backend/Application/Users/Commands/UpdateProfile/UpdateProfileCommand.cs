using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Users;
using MediatR;

namespace Gulora.Application.Users.Commands.UpdateProfile;

/// <summary>
/// Kullanıcı profilindeki temel kişisel bilgilerin güncellenmesini isteyen komut.
/// </summary>
/// <param name="UserId">Güncellenecek kullanıcının kimliği.</param>
/// <param name="FirstName">Yeni ad.</param>
/// <param name="LastName">Yeni soyad.</param>
/// <param name="Email">Yeni e-posta (benzersizlik iş kuralı ile kontrol edilir).</param>
public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email) : IRequest<UserDto>, ICommand;
