namespace Gulora.Contracts.Auth;

/// <summary>POST api/v1/auth/login gövdesi.</summary>
public sealed record LoginRequest(
    string Email,
    string Password);
