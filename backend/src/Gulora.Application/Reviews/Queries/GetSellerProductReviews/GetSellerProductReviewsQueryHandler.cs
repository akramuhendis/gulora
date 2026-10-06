using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetSellerProductReviews;

public sealed class GetSellerProductReviewsQueryHandler
    : IRequestHandler<GetSellerProductReviewsQuery, PagedResult<ReviewResponse>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public GetSellerProductReviewsQueryHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IUserRepository userRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ReviewResponse>> Handle(
        GetSellerProductReviewsQuery query,
        CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 50);
        var page = Math.Max(query.Page, 1);

        var product = await _productRepository.GetByIdAsync(query.ProductId, ct)
            ?? throw new NotFoundException("Ürün", query.ProductId);

        if (product.SellerId != query.SellerUserId)
            throw new ForbiddenException("Bu məhsulun rəylərinə baxmaq üçün icazəniz yoxdur.");

        var totalCount = await _reviewRepository.GetTotalCountByProductAsync(query.ProductId, ct);
        var reviews = await _reviewRepository.GetByProductPagedAsync(
            query.ProductId, page, pageSize, ct);

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
