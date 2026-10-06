using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.RefreshToken;

/// <summary>Mevcut refresh token ile yeni JWT çifti alır.</summary>
public sealed record RefreshTokenCommand(
    string Token
) : IRequest<AuthResponse>;
