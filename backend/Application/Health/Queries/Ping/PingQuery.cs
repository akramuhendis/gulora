using MediatR;

namespace Gulora.Application.Health.Queries.Ping;

public sealed record PingQuery : IRequest<string>;
