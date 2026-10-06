using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.VerifyOtp;

public sealed record VerifyOtpCommand(string PhoneNumber, string Code, Guid? LinkUserId)
    : IRequest<OtpResponse>;
