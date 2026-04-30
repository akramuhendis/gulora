using Gulora.Domain.Common;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Bir ürün için müşteri değerlendirmesi (yıldız + yorum).
/// <see cref="IsVerified"/> genelde “satın aldı doğrulandı” gibi kurallarla uygulama katmanında set edilir.
/// </summary>
public class Review : BaseEntity
{
    /// <summary>Değerlendirilen ürün.</summary>
    public Guid ProductId { get; private set; }
    /// <summary>Yorumu yazan kullanıcı.</summary>
    public Guid UserId { get; private set; }
    /// <summary>1–5 arası yıldız sayısı.</summary>
    public int Rating { get; private set; }
    /// <summary>Metin yorumu.</summary>
    public string Comment { get; private set; } = default!;
    /// <summary>Gerçek alıcı doğrulaması yapıldıysa true.</summary>
    public bool IsVerified { get; private set; }
    /// <summary>İstifadəçi tərəfindən silinmiş (soft delete).</summary>
    public bool IsDeleted { get; private set; }
    /// <summary>Moderator tərəfindən gizlədilmiş.</summary>
    public bool IsHidden { get; private set; }

    private Review() { }

    /// <summary>Yeni yorum oluşturur.</summary>
    public static Review Create(Guid productId, Guid userId, int rating, string comment)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        if (rating is < 1 or > 5)
            throw new DomainException("Reytinq 1 ilə 5 arasında olmalıdır.");
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        var review = new Review { ProductId = productId, UserId = userId, Rating = rating, Comment = comment.Trim() };
        review.RegisterDomainEvent(new ReviewCreatedDomainEvent(review.Id, productId, userId));
        return review;
    }

    /// <summary>Məhsul səhifəsində siyahıya düşməli rəylər (silinməmiş və gizlədilməmiş).</summary>
    public bool IsListedInCatalog() => !IsDeleted && !IsHidden;

    /// <summary>Moderasyon və ya sipariş doğrulaması sonrası güven rozetini açar (idempotent).</summary>
    public void Verify()
    {
        if (IsVerified) return;
        if (IsDeleted)
            throw new DomainException("Silinmiş rəy doğrulanmır.");
        IsVerified = true;
        SetUpdated();
    }

    /// <summary>Müştəri öz rəyini redaktə edir.</summary>
    public void UpdateContent(int rating, string comment)
    {
        if (rating is < 1 or > 5)
            throw new DomainException("Reytinq 1 ilə 5 arasında olmalıdır.");
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);
        EnsureVisibleForMutation();
        Rating = rating;
        Comment = comment.Trim();
        SetUpdated();
    }

    /// <summary>Moderator kataloqda gizlədir.</summary>
    public void HideByModeration()
    {
        if (IsHidden) return;
        EnsureNotDeletedForModeration();
        IsHidden = true;
        SetUpdated();
    }

    /// <summary>Moderator gizlətməni götürür.</summary>
    public void UnhideByModeration()
    {
        if (!IsHidden) return;
        EnsureNotDeletedForModeration();
        IsHidden = false;
        SetUpdated();
    }

    /// <summary>İstifadəçi rəyi silir (soft delete).</summary>
    public void MarkDeletedByUser()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        SetUpdated();
    }

    private void EnsureVisibleForMutation()
    {
        if (IsDeleted)
            throw new DomainException("Silinmiş rəy yenilənə bilməz.");
        if (IsHidden)
            throw new DomainException("Gizlədilmiş rəy yenilənə bilməz.");
    }

    private void EnsureNotDeletedForModeration()
    {
        if (IsDeleted)
            throw new DomainException("Silinmiş rəy üzərində moderasiya əməliyyatı aparıla bilməz.");
    }
}
