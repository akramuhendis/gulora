using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Auth;
using Gulora.Contracts.Users;
using Gulora.Domain.Exceptions;
using MediatR;
using DomainRefreshToken = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.Login;

/// <summary>
/// E-posta ve parola ile oturum açar; erişim ve yenileme jetonları ile kullanıcı DTO'su döner.
/// </summary>
public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtService jwt,
    IRefreshTokenRepository refreshTokens,
    IMapper mapper) : IRequestHandler<LoginCommand, LoginResponse>
{
    private const int RefreshTokenLifetimeDays = 30;

    /// <summary>
    /// Kimlik bilgilerini doğrular, hesap etkinliğini kontrol eder, eski yenileme jetonlarını iptal eder ve yeni çift üretir.
    /// </summary>
    /// <param name="request">E-posta ve düz metin parola.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Erişim jetonu, düz yenileme jetonu ve kullanıcı DTO'su.</returns>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // E-postayı normalize et (karşılaştırma ve depolama tutarlılığı için).
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("The account is disabled.");

        // Güvenlik için kullanıcıya ait tüm önceki yenileme jetonlarını geçersiz kıl.
        await refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken).ConfigureAwait(false);

        // Yeni erişim jetonu ve veritabanında saklanacak karma yenileme kaydı oluştur.
        var access = jwt.GenerateAccessToken(user);
        var refreshPlain = jwt.GenerateRefreshToken();
        var refresh = DomainRefreshToken.Create(user.Id, refreshPlain, RefreshTokenLifetimeDays);
        await refreshTokens.AddAsync(refresh, cancellationToken).ConfigureAwait(false);

        await users.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = mapper.Map<UserDto>(user);
        return new LoginResponse(access, refreshPlain, dto);
    }
}
