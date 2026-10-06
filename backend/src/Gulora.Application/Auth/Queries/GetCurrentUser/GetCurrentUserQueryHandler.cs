using Gulora.Contracts.Auth;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", query.UserId);

        return new CurrentUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role.ToString(),
            user.IsActive,
            user.EmailVerified,
            user.PhoneVerified,
            user.LastLoginAt,
            user.CreatedAt);
    }
}
