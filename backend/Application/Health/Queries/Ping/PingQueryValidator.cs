using FluentValidation;

namespace Gulora.Application.Health.Queries.Ping;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
    }
}
