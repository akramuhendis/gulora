using Gulora.Domain.Common;
using Gulora.Domain.Enums;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Ürün yorumu ve puanı.
/// Fiziksel silme desteklenir (kullanıcı veya admin silebilir).
/// Moderasyon onayı olmadan yayınlanmaz — <see cref="ReviewStatus.Pending"/> başlangıç durumudur.
/// </summary>
public sealed class Review : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public int Rating { get; private set; }
    public string? Title { get; private set; }
    public string? Body { get; private set; }
    public ReviewStatus Status { get; private set; } = ReviewStatus.Pending;
    public string? RejectionReason { get; private set; }

    private Review() { }

    /// <summary>
    /// Yeni yorum oluşturur ve <see cref="ReviewSubmittedEvent"/> yayınlar.
    /// Rating 1–5 arasında olmalıdır.
    /// </summary>
    public static Review Create(
        Guid productId,
        Guid userId,
        int rating,
        string? title = null,
        string? body = null,
        Guid? orderId = null)
    {
        if (rating is < 1 or > 5)
            throw new BusinessRuleException("Puan 1 ile 5 arasında olmalıdır.");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Title = title?.Trim(),
            Body = body?.Trim(),
            OrderId = orderId,
            Status = ReviewStatus.Pending
        };

        review.AddDomainEvent(new ReviewSubmittedEvent(review.Id, productId, userId, rating));

        return review;
    }

    /// <summary>Yorumu onaylar (moderatör).</summary>
    public void Approve()
    {
        if (Status == ReviewStatus.Approved)
            throw new BusinessRuleException("Yorum zaten onaylanmış.");

        Status = ReviewStatus.Approved;
        RejectionReason = null;
        MarkUpdated();
    }

    /// <summary>Yorumu reddeder (moderatör).</summary>
    public void Reject(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Status = ReviewStatus.Rejected;
        RejectionReason = reason.Trim();
        MarkUpdated();
    }

    /// <summary>Yorum içeriğini günceller (kullanıcı tarafından, yeniden pending'e döner).</summary>
    public void Update(int rating, string? title, string? body)
    {
        if (rating is < 1 or > 5)
            throw new BusinessRuleException("Puan 1 ile 5 arasında olmalıdır.");

        Rating = rating;
        Title = title?.Trim();
        Body = body?.Trim();
        Status = ReviewStatus.Pending;
        RejectionReason = null;
        MarkUpdated();
    }

    /// <summary>Onaylanmış ve görünür bir yorum mu?</summary>
    public bool IsVisible => Status == ReviewStatus.Approved;
}
