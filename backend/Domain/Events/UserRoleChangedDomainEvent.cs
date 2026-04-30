using Gulora.Domain.Enums;

namespace Gulora.Domain.Events;

/// <summary>İstifadəçi rolu dəyişdikdə (məs. satıcı təsdiqi, admin təyinatı) yayınlanır.</summary>
public sealed record UserRoleChangedDomainEvent(
    Guid UserId,
    UserRole PreviousRole,
    UserRole NewRole) : DomainEventBase;
