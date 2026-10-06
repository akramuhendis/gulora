namespace Gulora.Contracts.Auth;

/// <summary>
/// GET api/v1/auth/me — token sahibinin özet bilgisi.
/// Adres listesi dahil değildir; tam profil için GET api/v1/users/me kullanılır.
/// </summary>
public sealed record CurrentUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    bool EmailVerified,
    bool PhoneVerified,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
