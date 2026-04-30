namespace Gulora.Contracts.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Password);
