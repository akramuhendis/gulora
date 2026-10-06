using MediatR;

namespace Gulora.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Parola sıfırlama e-postası gönderir.
/// E-posta kayıtlı değilse sessizce geçilir (kullanıcı bilgi toplama saldırısına karşı).
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest;
