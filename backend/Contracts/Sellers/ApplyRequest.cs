namespace Gulora.Contracts.Sellers;

public record ApplyRequest(
    string FullName,
    string Company,
    string TaxNumber,
    string Email,
    string Phone,
    string City,
    string Category,
    string Message);
