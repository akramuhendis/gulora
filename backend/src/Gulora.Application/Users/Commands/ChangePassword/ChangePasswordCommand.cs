using MediatR;

namespace Gulora.Application.Users.Commands.ChangePassword;

/// <summary>Mevcut parolayı doğrulayarak yeni parola belirler.</summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest;
