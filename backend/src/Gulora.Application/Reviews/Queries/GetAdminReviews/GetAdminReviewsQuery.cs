using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetAdminReviews;

/// <summary>
/// Admin moderasyon paneli için yorumları sayfali getirir.
/// Durum (Pending / Approved / Rejected) ve ürün ID'si ile filtreleme desteklenir.
/// </summary>
public sealed record GetAdminReviewsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? ProductId = null
) : IRequest<PagedResult<ReviewResponse>>;
