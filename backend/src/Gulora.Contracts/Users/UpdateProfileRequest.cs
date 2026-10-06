namespace Gulora.Contracts.Users;

/// <summary>PUT api/v1/users/me gövdesi.</summary>
public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);
