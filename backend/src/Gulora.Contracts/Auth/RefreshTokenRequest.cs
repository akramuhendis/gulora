namespace Gulora.Contracts.Auth;

/// <summary>POST api/v1/auth/refresh gövdesi.</summary>
public sealed record RefreshTokenRequest(
    string RefreshToken);
