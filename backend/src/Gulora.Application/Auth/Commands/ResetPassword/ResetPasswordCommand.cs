using MediatR;

namespace Gulora.Application.Auth.Commands.ResetPassword;

/// <summary>Parola sıfırlama token'ıyla yeni parola belirler.</summary>
public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest;
