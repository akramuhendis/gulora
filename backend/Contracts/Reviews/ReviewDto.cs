namespace Gulora.Contracts.Reviews;

public record ReviewDto(
    Guid Id,
    string UserFullName,
    int Rating,
    string Comment,
    bool IsVerified,
    DateTime CreatedAt);
