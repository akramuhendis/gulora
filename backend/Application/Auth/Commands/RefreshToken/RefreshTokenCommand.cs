using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Süresi dolmamış/geçerli yenileme jetonu ile yeni oturum bileşenleri almayı isteyen komut.
/// </summary>
/// <param name="RefreshToken">Sunucuda doğrulanacak düz metin yenileme jetonu.</param>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>, ICommand;
