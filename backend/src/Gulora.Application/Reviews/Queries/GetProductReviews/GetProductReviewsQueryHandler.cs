using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetProductReviews;

public sealed class GetProductReviewsQueryHandler
    : IRequestHandler<GetProductReviewsQuery, PagedResult<ReviewResponse>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;

    public GetProductReviewsQueryHandler(
        IReviewRepository reviewRepository,
        IUserRepository userRepository)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ReviewResponse>> Handle(
        GetProductReviewsQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 50);
        var page = Math.Max(query.Page, 1);

        var reviews = await _reviewRepository.GetApprovedByProductPagedAsync(
            query.ProductId, page, pageSize, ct);

        var totalCount = await _reviewRepository.GetApprovedCountByProductAsync(query.ProductId, ct);

        // N+1 önleme: kullanıcı kimliklerini toplu çek
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
