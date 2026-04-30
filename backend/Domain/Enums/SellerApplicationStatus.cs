namespace Gulora.Domain.Enums;

/// <summary>
/// Satıcı olmak için yapılan başvurunun işlem durumu.
/// </summary>
public enum SellerApplicationStatus
{
    /// <summary>Form gönderildi, henüz incelenmedi.</summary>
    Pending = 0,
    /// <summary>Operasyon veya uyum ekibi incelemesi devam ediyor.</summary>
    UnderReview = 1,
    /// <summary>Başvuru kabul edildi; onboarding adımları başlayabilir.</summary>
    Approved = 2,
    /// <summary>Başvuru reddedildi.</summary>
    Rejected = 3
}
