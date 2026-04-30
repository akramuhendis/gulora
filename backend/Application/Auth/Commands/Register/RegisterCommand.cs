using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.Register;

/// <summary>
/// Sistemde yeni bir kullanıcı kaydı oluşturmayı isteyen komut.
/// </summary>
/// <param name="FirstName">Kullanıcının adı.</param>
/// <param name="LastName">Kullanıcının soyadı.</param>
/// <param name="Email">Benzersiz olması beklenen e-posta adresi.</param>
/// <param name="Phone">İsteğe bağlı telefon numarası.</param>
/// <param name="Password">Hesap parolası (sunucuda hashlenir).</param>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Password) : IRequest<RegisterResponse>, ICommand;
