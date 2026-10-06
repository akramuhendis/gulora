using Gulora.Contracts.Common;
using Gulora.Contracts.SellerApplications;
using Gulora.Domain.Enums;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerApplications.Queries.GetAdminSellerApplications;

public sealed record GetAdminSellerApplicationsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null
) : IRequest<PagedResult<SellerApplicationResponse>>;

public sealed class GetAdminSellerApplicationsQueryHandler
    : IRequestHandler<GetAdminSellerApplicationsQuery, PagedResult<SellerApplicationResponse>>
{
    private readonly ISellerApplicationRepository _repository;

    public GetAdminSellerApplicationsQueryHandler(ISellerApplicationRepository repository)
        => _repository = repository;

    public async Task<PagedResult<SellerApplicationResponse>> Handle(
        GetAdminSellerApplicationsQuery query,
        CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        SellerApplicationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<SellerApplicationStatus>(query.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        var apps = await _repository.ListPagedAsync(
            a => !statusFilter.HasValue || a.Status == statusFilter.Value,
            page,
            pageSize,
            ct);

        var total = await _repository.CountAsync(
            a => !statusFilter.HasValue || a.Status == statusFilter.Value,
            ct);

        var items = apps.Select(a => new SellerApplicationResponse(
            a.Id,
            a.FullName,
            a.Company,
            a.Email,
            a.Status.ToString(),
            a.RejectionReason,
            a.CreatedAt,
            a.UpdatedAt)).ToList();

        return new PagedResult<SellerApplicationResponse>(items, total, page, pageSize);
    }
}
