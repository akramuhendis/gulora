using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Users.Commands.ChangePassword;

/// <summary>
/// Kimliği doğrulanmış kullanıcı için parola değişikliği isteği.
/// </summary>
/// <param name="UserId">Parolası değiştirilecek kullanıcının kimliği.</param>
/// <param name="CurrentPassword">Doğrulama için mevcut düz metin parola.</param>
/// <param name="NewPassword">Yerine yazılacak yeni düz metin parola (sunucuda hashlenir).</param>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<Unit>, ICommand;
