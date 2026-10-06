using Gulora.Contracts.Common;
using Gulora.Contracts.Users;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Users.Queries.GetAdminUsers;

public sealed class GetAdminUsersQueryHandler
    : IRequestHandler<GetAdminUsersQuery, PagedResult<UserSummaryResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetAdminUsersQueryHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<PagedResult<UserSummaryResponse>> Handle(
        GetAdminUsersQuery query, CancellationToken ct)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var allUsers = await _userRepository.ListPagedAsync(
            string.IsNullOrWhiteSpace(query.SearchTerm)
                ? null
                : u => u.FirstName.Contains(query.SearchTerm)
                    || u.LastName.Contains(query.SearchTerm)
                    || u.Email.Contains(query.SearchTerm),
            page,
            pageSize,
            ct);

        var totalCount = await _userRepository.CountAsync(
            string.IsNullOrWhiteSpace(query.SearchTerm)
                ? null
                : u => u.FirstName.Contains(query.SearchTerm)
                    || u.LastName.Contains(query.SearchTerm)
                    || u.Email.Contains(query.SearchTerm),
            ct);

        var items = allUsers.Select(u => new UserSummaryResponse(
            u.Id,
            u.FullName,
            u.Email,
            u.PhoneNumber,
            u.Role.ToString(),
            u.IsActive,
            u.EmailVerified,
            u.LastLoginAt,
            u.CreatedAt)).ToList();

        return new PagedResult<UserSummaryResponse>(items, totalCount, page, pageSize);
    }
}
