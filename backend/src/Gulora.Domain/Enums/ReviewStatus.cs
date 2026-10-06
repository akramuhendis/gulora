namespace Gulora.Domain.Enums;

/// <summary>Ürün yorumunun moderasyon durumu.</summary>
public enum ReviewStatus
{
    /// <summary>Moderasyon bekliyor.</summary>
    Pending = 1,

    /// <summary>Yayınlandı.</summary>
    Approved = 2,

    /// <summary>Moderasyon tarafından reddedildi.</summary>
    Rejected = 3
}
