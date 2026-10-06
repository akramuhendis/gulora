using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Commands.SendOtp;

public sealed record SendOtpCommand(string PhoneNumber) : IRequest<OtpResponse>;
