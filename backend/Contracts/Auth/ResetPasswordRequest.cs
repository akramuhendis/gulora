namespace Gulora.Contracts.Auth;

public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);
