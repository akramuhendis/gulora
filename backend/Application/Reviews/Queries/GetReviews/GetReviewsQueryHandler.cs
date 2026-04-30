using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetReviews;

/// <summary>
/// Ürün kimliğine göre yorumları sayfalar; her yorum için görünen kullanıcı adını toplu kullanıcı sorgusu ile üretir.
/// </summary>
/// <remarks>
/// Yorum başına ayrı kullanıcı sorgusu (N+1) yerine benzersiz <c>UserId</c> listesi ile tek toplu yükleme yapılır.
/// </remarks>
public sealed class GetReviewsQueryHandler(
    IReviewRepository reviews,
    IUserRepository users) : IRequestHandler<GetReviewsQuery, PagedResult<ReviewDto>>
{
    /// <summary>
    /// Depodan sayfalı yorumları alır, kullanıcı sözlüğü ile <see cref="ReviewDto"/> listesi oluşturur.
    /// </summary>
    /// <param name="request">Ürün ve sayfalama parametreleri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Sayfalanmış yorum DTO'ları ve toplam kayıt sayısı.</returns>
    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await reviews.GetByProductIdAsync(
            request.ProductId, request.Page, request.PageSize, cancellationToken).ConfigureAwait(false);

        // Tek tek kullanıcı sorgusu yerine benzersiz kullanıcı kimlikleriyle toplu yükleme
        var userIds = items.Select(r => r.UserId).Distinct().ToList();
        var loadedUsers = await users.GetByIdsAsync(userIds, cancellationToken).ConfigureAwait(false);
        var userById = loadedUsers.ToDictionary(u => u.Id);

        var dtos = items.Select(r =>
        {
            // Kullanıcı silinmiş veya bulunamıyorsa sabit anonim metin kullan
            var fullName = userById.TryGetValue(r.UserId, out var u)
                ? $"{u.FirstName} {u.LastName}"
                : ReviewDisplayMessages.AnonymousUser;
            return new ReviewDto(r.Id, fullName, r.Rating, r.Comment, r.IsVerified, r.CreatedAt);
        }).ToList();

        return new PagedResult<ReviewDto>(dtos, total, request.Page, request.PageSize);
    }
}
