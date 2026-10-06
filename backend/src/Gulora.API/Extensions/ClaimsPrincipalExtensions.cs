using System.Security.Claims;
using Gulora.Domain.Exceptions;

namespace Gulora.API.Extensions;

/// <summary>
/// ClaimsPrincipal yardımcı uzantıları — JWT claim'lerinden kullanıcı bilgisi çeker.
/// Token'da "sub" veya ClaimTypes.NameIdentifier kullanıldığı ASP.NET Core JWT davranışına uygun
/// çift kontrol yapılır (CurrentUserService ile aynı felsefede).
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// JWT token'dan kullanıcı Guid'ini döner.
    /// Token yoksa veya parse edilemezse <see cref="UnauthorizedException"/> fırlatır.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedException("Kullanıcı kimliği bulunamadı.");
    }

    /// <summary>
    /// JWT token'dan kullanıcı Guid'ini döner; yoksa <c>null</c>.
    /// </summary>
    public static Guid? TryGetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

        return Guid.TryParse(value, out var id) ? id : null;
    }

    /// <summary>Kullanıcının belirtilen role sahip olup olmadığını kontrol eder.</summary>
    public static bool IsInRole(this ClaimsPrincipal principal, string role)
        => principal.FindFirstValue(ClaimTypes.Role)
               ?.Equals(role, StringComparison.OrdinalIgnoreCase) == true;
}
