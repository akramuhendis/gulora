using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Users;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Users.Commands.UpdateProfile;

/// <summary>
/// Kullanıcının ad, soyad ve e-posta bilgilerini günceller; e-posta çakışmasını önler.
/// </summary>
public sealed class UpdateProfileCommandHandler(
    IUserRepository users,
    IMapper mapper) : IRequestHandler<UpdateProfileCommand, UserDto>
{
    /// <summary>
    /// Kullanıcıyı yükler, e-posta değiştiyse başka hesapta kullanılıp kullanılmadığını kontrol eder, profili günceller ve DTO döner.
    /// </summary>
    /// <param name="request">Kullanıcı kimliği ve yeni profil alanları.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Güncellenmiş kullanıcı DTO'su.</returns>
    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        var newEmail = request.Email.Trim().ToLowerInvariant();
        // E-posta gerçekten değişiyorsa, yeni adresin başka bir hesapta kullanılmadığını doğrula.
        if (!string.Equals(user.Email, newEmail, StringComparison.Ordinal))
        {
            if (await users.ExistsAsync(newEmail, cancellationToken).ConfigureAwait(false))
                throw new DomainException("This email address is already in use by another account.");
        }

        user.UpdateProfile(request.FirstName, request.LastName, newEmail);
        await users.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<UserDto>(user);
    }
}
