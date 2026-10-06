using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.Login;

/// <summary>Kullanıcı girişi — JWT çifti döner.</summary>
public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
