using AutoMapper;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Auth;
using Gulora.Contracts.Users;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;
using DomainRefreshToken = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.Register;

/// <summary>
/// Yeni kullanıcı hesabı oluşturur, parolayı hashler ve ilk oturum için jeton çifti üretir.
/// </summary>
public sealed class RegisterCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtService jwt,
    IRefreshTokenRepository refreshTokens,
    IMapper mapper) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private const int RefreshTokenLifetimeDays = 30;

    /// <summary>
    /// E-posta çakışmasını kontrol eder, kullanıcıyı oluşturur, isteğe bağlı telefonu ayarlar, jetonları yazar ve yanıt döner.
    /// </summary>
    /// <param name="request">Profil alanları, e-posta, isteğe bağlı telefon ve parola.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Kayıt sonrası erişim/yenileme jetonları ve kullanıcı DTO'su.</returns>
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.ExistsAsync(email, cancellationToken).ConfigureAwait(false))
            throw new DomainException("An account with this email address already exists.");

        // Parolayı düz metinden hashle; kullanıcı varlığı domain kurallarıyla oluşturulur.
        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.FirstName.Trim(), request.LastName.Trim(), email, hash);
        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.SetPhone(request.Phone);

        await users.AddAsync(user, cancellationToken).ConfigureAwait(false);

        // Kayıt sonrası otomatik oturum: erişim ve yenileme jetonları.
        var access = jwt.GenerateAccessToken(user);
        var refreshPlain = jwt.GenerateRefreshToken();
        var refresh = DomainRefreshToken.Create(user.Id, refreshPlain, RefreshTokenLifetimeDays);
        await refreshTokens.AddAsync(refresh, cancellationToken).ConfigureAwait(false);

        await users.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = mapper.Map<UserDto>(user);
        return new RegisterResponse(access, refreshPlain, dto);
    }
}
