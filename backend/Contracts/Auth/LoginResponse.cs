using Gulora.Contracts.Users;

namespace Gulora.Contracts.Auth;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User);
