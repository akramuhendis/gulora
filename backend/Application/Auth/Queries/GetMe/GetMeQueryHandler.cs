using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Users;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Auth.Queries.GetMe;

/// <summary>
/// Kimliği verilen kullanıcıyı yükler ve <see cref="UserDto"/> olarak döndürür.
/// </summary>
public sealed class GetMeQueryHandler(
    IUserRepository users,
    IMapper mapper) : IRequestHandler<GetMeQuery, UserDto>
{
    /// <summary>
    /// Kullanıcıyı depodan getirir; bulunamazsa hata fırlatır, bulunursa eşlenmiş DTO döner.
    /// </summary>
    /// <param name="request">Sorgulanacak kullanıcı kimliği.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Kullanıcı profil DTO'su.</returns>
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        return mapper.Map<UserDto>(user);
    }
}
