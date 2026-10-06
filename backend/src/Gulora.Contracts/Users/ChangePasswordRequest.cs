namespace Gulora.Contracts.Users;

/// <summary>POST api/v1/users/me/change-password gövdesi.</summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
