using MediatR;

namespace Gulora.Application.Health.Queries.Ping;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, string>
{
    public Task<string> Handle(PingQuery request, CancellationToken cancellationToken) =>
        Task.FromResult("ok");
}
