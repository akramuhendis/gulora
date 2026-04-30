using System.Security.Claims;
using Gulora.Domain.Exceptions;

namespace Gulora.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var v = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(v) || !Guid.TryParse(v, out var id))
            throw new UnauthorizedException("Token etibarsızdır.");
        return id;
    }
}
