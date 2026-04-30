namespace Gulora.Contracts.Users;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsPhoneVerified,
    string Role,
    DateTime CreatedAt);
