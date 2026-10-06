namespace Gulora.Contracts.Users;

/// <summary>GET api/v1/users/me — tam profil yanıtı (adresler dahil).</summary>
public sealed record UserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    bool EmailVerified,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    IReadOnlyList<AddressResponse> Addresses);
