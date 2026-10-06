using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetProductReviews;

/// <summary>Ürüne ait onaylanmış yorumları sayfalı getirir.</summary>
public sealed record GetProductReviewsQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<ReviewResponse>>;
