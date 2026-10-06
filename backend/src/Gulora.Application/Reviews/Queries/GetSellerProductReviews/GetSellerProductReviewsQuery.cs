using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetSellerProductReviews;

public sealed record GetSellerProductReviewsQuery(
    Guid SellerUserId,
    Guid ProductId,
    int Page,
    int PageSize) : IRequest<PagedResult<ReviewResponse>>;
