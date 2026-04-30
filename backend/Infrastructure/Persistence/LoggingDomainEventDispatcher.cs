using System.Text.Json;
using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Gulora.Infrastructure.Persistence;

/// <summary>
/// Hazırda hadisələri strukturlaşdırılmış jurnalda saxlayır; sonradan MediatR və ya mesaj brokeri ilə əvəz oluna bilər.
/// </summary>
public sealed class LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger) : IDomainEventDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var e in events)
        {
            var payload = JsonSerializer.Serialize(e, e.GetType(), JsonOptions);
            logger.LogInformation(
                "Domain event {EventType} at {OccurredOn}: {Payload}",
                e.GetType().Name,
                e.OccurredOn,
                payload);
        }

        return Task.CompletedTask;
    }
}
