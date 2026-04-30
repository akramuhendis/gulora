using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.Login;

/// <summary>
/// Mevcut kullanıcı için oturum açma isteği (e-posta ve parola).
/// </summary>
/// <param name="Email">Kullanıcı e-posta adresi.</param>
/// <param name="Password">Düz metin parola (doğrulama sonrası hash ile karşılaştırılır).</param>
public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>, ICommand;
