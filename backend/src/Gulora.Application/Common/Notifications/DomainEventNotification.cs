using Gulora.Domain.Common;
using MediatR;

namespace Gulora.Application.Common.Notifications;

/// <summary>
/// Domain olayını MediatR INotification'a saran sarmalayıcı.
/// Domain katmanı MediatR'a bağımlı olmaz; Application bu köprüyü kurar.
/// Infrastructure SaveChanges sonrasında toplanan domain olaylarını
/// DomainEventNotification&lt;T&gt;'ye sararak MediatR üzerinden yayınlar.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent)
    : INotification
    where TEvent : IDomainEvent;
