using Gulora.Contracts.Users;
using MediatR;

namespace Gulora.Application.Users.Queries.GetMyProfile;

/// <summary>Oturum açmış kullanıcının tam profilini adresleriyle birlikte getirir.</summary>
public sealed record GetMyProfileQuery(Guid UserId) : IRequest<UserProfileResponse>;
