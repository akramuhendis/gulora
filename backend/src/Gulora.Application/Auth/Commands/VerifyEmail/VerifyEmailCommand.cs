using MediatR;

namespace Gulora.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest;
