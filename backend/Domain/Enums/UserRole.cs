namespace Gulora.Domain.Enums;

/// <summary>
/// Kullanıcının sistem içi rolü; JWT taleplerinde ve yetki kontrollerinde kullanılır.
/// </summary>
public enum UserRole
{
    /// <summary>Alışveriş yapan son kullanıcı.</summary>
    Customer = 0,
    /// <summary>Yönetim paneli ve operasyon yetkileri.</summary>
    Admin = 1,
    /// <summary>Mağaza / satıcı hesabı (onaylı başvuru sonrası).</summary>
    Seller = 2
}
