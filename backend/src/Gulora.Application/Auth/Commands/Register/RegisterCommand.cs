using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.Register;

/// <summary>
/// Yeni kullanıcı kaydı.
/// Başarılı olursa JWT çifti (AccessToken + RefreshToken) döner.
/// </summary>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber = null
) : IRequest<AuthResponse>;
