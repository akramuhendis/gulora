using Gulora.Contracts.Users;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Users.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileResponse>
{
    private readonly IUserRepository _userRepository;

    public GetMyProfileQueryHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<UserProfileResponse> Handle(GetMyProfileQuery query, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", query.UserId);

        var addresses = user.Addresses.Select(a => new AddressResponse(
            a.Id,
            a.Title,
            a.RecipientName,
            a.PhoneNumber,
            a.City,
            a.District,
            a.FullLine,
            a.PostalCode,
            a.CountryCode,
            a.IsDefault)).ToList();

        return new UserProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role.ToString(),
            user.IsActive,
            user.EmailVerified,
            user.LastLoginAt,
            user.CreatedAt,
            addresses);
    }
}
