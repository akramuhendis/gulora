using Gulora.Contracts.Auth;
using MediatR;

namespace Gulora.Application.Auth.Queries.GetCurrentUser;

/// <summary>Token'dan alınan kullanıcı kimliğiyle mevcut kullanıcı bilgisini döner.</summary>
public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResponse>;
