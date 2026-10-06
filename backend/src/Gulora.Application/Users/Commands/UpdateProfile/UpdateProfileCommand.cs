using MediatR;

namespace Gulora.Application.Users.Commands.UpdateProfile;

/// <summary>Oturum açmış kullanıcının profil bilgilerini günceller.</summary>
public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : IRequest;
