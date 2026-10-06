namespace Gulora.Contracts.Auth;

/// <summary>POST api/v1/auth/register gövdesi.</summary>
public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber = null);
