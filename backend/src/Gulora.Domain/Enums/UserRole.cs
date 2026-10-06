namespace Gulora.Domain.Enums;

/// <summary>Sistemdeki kullanıcı rolleri.</summary>
public enum UserRole
{
    /// <summary>Son tüketici — alışveriş yapan müşteri.</summary>
    Customer = 1,

    /// <summary>Platform yöneticisi — tam yetki.</summary>
    Admin = 2,

    /// <summary>Ürün listeleyen satıcı.</summary>
    Seller = 3
}
