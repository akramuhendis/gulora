namespace Gulora.Contracts.Auth;

public record VerifyOtpRequest(string PhoneNumber, string Code);
