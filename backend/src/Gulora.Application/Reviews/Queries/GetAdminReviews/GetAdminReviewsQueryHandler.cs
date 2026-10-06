using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using Gulora.Domain.Enums;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetAdminReviews;

public sealed class GetAdminReviewsQueryHandler
    : IRequestHandler<GetAdminReviewsQuery, PagedResult<ReviewResponse>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;

    public GetAdminReviewsQueryHandler(
        IReviewRepository reviewRepository,
        IUserRepository userRepository)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ReviewResponse>> Handle(
        GetAdminReviewsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        ReviewStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<ReviewStatus>(query.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        // Durum ve ürün filtresi uygulayarak sayfali veri çek
        var reviews = await _reviewRepository.ListPagedAsync(
            r => (!statusFilter.HasValue || r.Status == statusFilter.Value) &&
                 (!query.ProductId.HasValue || r.ProductId == query.ProductId.Value),
            page,
            pageSize,
            ct);

        var totalCount = await _reviewRepository.CountAsync(
            r => (!statusFilter.HasValue || r.Status == statusFilter.Value) &&
                 (!query.ProductId.HasValue || r.ProductId == query.ProductId.Value),
            ct);

        // Kullanıcı adlarını toplu çek (N+1 önleme)
        var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
        var userNames = new Dictionary<Guid, string?>(userIds.Count);
        foreach (var uid in userIds)
        {
            var user = await _userRepository.GetByIdAsync(uid, ct);
            userNames[uid] = user?.FullName;
        }

        var items = reviews.Select(r => new ReviewResponse(
            r.Id,
            r.ProductId,
            r.UserId,
            r.OrderId,
            userNames.GetValueOrDefault(r.UserId),
            r.Rating,
            r.Title,
            r.Body,
            r.Status.ToString(),
            r.RejectionReason,
            r.CreatedAt,
            r.UpdatedAt)).ToList();

        return new PagedResult<ReviewResponse>(items, totalCount, page, pageSize);
    }
}
