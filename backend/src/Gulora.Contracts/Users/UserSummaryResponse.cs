namespace Gulora.Contracts.Users;

/// <summary>
/// Admin kullanıcı listesi — GET api/v1/admin/users.
/// Kişisel detaylar (PasswordHash, token vb.) bu yanıtta kesinlikle yer almaz.
/// </summary>
public sealed record UserSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    bool EmailVerified,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
