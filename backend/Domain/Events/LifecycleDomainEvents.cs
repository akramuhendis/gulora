namespace Gulora.Domain.Events;

public sealed record ProductCreatedDomainEvent(Guid ProductId) : DomainEventBase;

public sealed record ReviewCreatedDomainEvent(Guid ReviewId, Guid ProductId, Guid UserId) : DomainEventBase;

public sealed record CouponCreatedDomainEvent(Guid CouponId, string Code) : DomainEventBase;

public sealed record CorporateInquirySubmittedDomainEvent(Guid InquiryId) : DomainEventBase;

public sealed record AddressCreatedDomainEvent(Guid AddressId, Guid UserId) : DomainEventBase;

public sealed record CategoryCreatedDomainEvent(Guid CategoryId) : DomainEventBase;

public sealed record FavoriteAddedDomainEvent(Guid FavoriteId, Guid UserId, Guid ProductId) : DomainEventBase;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Email) : DomainEventBase;

public sealed record SellerApplicationSubmittedDomainEvent(Guid ApplicationId) : DomainEventBase;

public sealed record SellerApplicationApprovedDomainEvent(Guid ApplicationId) : DomainEventBase;

public sealed record SellerApplicationRejectedDomainEvent(Guid ApplicationId, string? Reason) : DomainEventBase;
