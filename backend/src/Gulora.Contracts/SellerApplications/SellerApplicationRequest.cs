namespace Gulora.Contracts.SellerApplications;

/// <summary>
/// POST api/v1/seller-applications gövdesi — satıcı başvuru formu.
/// Kimlik doğrulama zorunlu değildir; başvuru sonrası admin onay süreci başlar.
/// Başvuru durumu <c>SellerApplicationResponse</c> ile takip edilebilir.
/// </summary>
public sealed record SellerApplicationRequest(
    string FullName,
    string Company,
    string TaxNumber,
    string Email,
    string Phone,
    string City,
    string Category,
    string? Message = null);
