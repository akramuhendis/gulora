namespace Gulora.Application.Common.Configuration;

/// <summary>
/// appsettings.json içində <c>Reviews</c> bölməsi — yorumların yayımlanma siyasəti.
/// </summary>
public sealed class ReviewPublishOptions
{
    public const string SectionName = "Reviews";

    /// <summary>
    /// Doğru olduğunda yeni yorum dərhal <c>Approved</c> olur və məhsul reytinqi yenilənir.
    /// Production-da adətən false; Development faylında true tövsiyə olunur.
    /// </summary>
    public bool AutoPublishOnSubmit { get; set; }

    /// <summary>
    /// Yalnız Development: tətbiq işə düşəndə bazadakı bütün <c>Pending</c> rəyləri bir dəfə təsdiq edir.
    /// Köhnə test rəylərini "gözləmədə" saxlamamaq üçün.
    /// </summary>
    public bool DevApprovePendingReviewsOnStartup { get; set; }
}
