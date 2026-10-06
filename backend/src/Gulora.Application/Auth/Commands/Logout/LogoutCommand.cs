using MediatR;

namespace Gulora.Application.Auth.Commands.Logout;

/// <summary>Kullanıcının aktif refresh token'ını iptal eder.</summary>
public sealed record LogoutCommand(
    string RefreshToken
) : IRequest;
