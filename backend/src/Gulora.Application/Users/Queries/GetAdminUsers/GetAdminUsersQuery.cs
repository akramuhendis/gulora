using Gulora.Contracts.Common;
using Gulora.Contracts.Users;
using MediatR;

namespace Gulora.Application.Users.Queries.GetAdminUsers;

/// <summary>Admin paneli — tüm kullanıcıları sayfalı getirir.</summary>
public sealed record GetAdminUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null
) : IRequest<PagedResult<UserSummaryResponse>>;
