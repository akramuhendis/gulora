namespace Gulora.Contracts.Auth;

/// <summary>POST api/v1/auth/logout gövdesi — aktif refresh token'ı iptal eder.</summary>
public sealed record LogoutRequest(
    string RefreshToken);
