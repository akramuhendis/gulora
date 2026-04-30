using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Auth;
using Gulora.Contracts.Users;
using Gulora.Domain.Exceptions;
using MediatR;
using DomainRefreshToken = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Geçerli bir yenileme jetonu ile yeni erişim jetonu ve döndürülebilir yenileme jetonu üretir (döndürme/rotation).
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    IJwtService jwt,
    IMapper mapper) : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private const int RefreshTokenLifetimeDays = 30;

    /// <summary>
    /// Yenileme jetonunu doğrular, kullanıcıyı yükler, eski jetonu iptal eder, yeni jeton çifti oluşturur ve <see cref="LoginResponse"/> döner.
    /// </summary>
    /// <param name="request">İstemciden gelen düz yenileme jetonu değeri.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Yeni erişim jetonu, yeni düz yenileme jetonu ve kullanıcı DTO'su.</returns>
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await refreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken).ConfigureAwait(false);
        if (existing is null || !existing.IsValid)
            throw new UnauthorizedException("The refresh token is invalid.");

        var user = await users.GetByIdAsync(existing.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.IsActive)
            throw new UnauthorizedException("User was not found or is inactive.");

        // Tek kullanımlık yenileme: mevcut kaydı iptal et, yenisini ekle.
        existing.Revoke();

        var access = jwt.GenerateAccessToken(user);
        var refreshPlain = jwt.GenerateRefreshToken();
        var newRefresh = DomainRefreshToken.Create(user.Id, refreshPlain, RefreshTokenLifetimeDays);
        await refreshTokens.AddAsync(newRefresh, cancellationToken).ConfigureAwait(false);
        await refreshTokens.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = mapper.Map<UserDto>(user);
        return new LoginResponse(access, refreshPlain, dto);
    }
}
