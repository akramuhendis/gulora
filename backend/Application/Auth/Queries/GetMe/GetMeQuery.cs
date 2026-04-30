using Gulora.Contracts.Users;
using MediatR;

namespace Gulora.Application.Auth.Queries.GetMe;

/// <summary>
/// Oturum açmış kullanıcının profil bilgilerini kimlik ile getirmek için sorgu.
/// </summary>
/// <param name="UserId">Profili istenen kullanıcının benzersiz kimliği.</param>
public sealed record GetMeQuery(Guid UserId) : IRequest<UserDto>;
