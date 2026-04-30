using Gulora.Contracts.Users;

namespace Gulora.Contracts.Auth;

public record RegisterResponse(string AccessToken, string RefreshToken, UserDto User);
