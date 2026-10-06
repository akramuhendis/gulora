using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Auth;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(cmd.Token, ct)
            ?? throw new UnauthorizedException("Refresh token geçersiz veya süresi dolmuş.");

        var existingToken = user.RefreshTokens
            .FirstOrDefault(t => t.Token == cmd.Token && t.IsActive)
            ?? throw new UnauthorizedException("Refresh token geçersiz veya süresi dolmuş.");

        if (!user.IsActive)
            throw new UnauthorizedException("Hesabınız devre dışı bırakılmış.");

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var newRefreshToken = RefreshTokenEntity.Create(user.Id, newRefreshTokenValue, DateTime.UtcNow.AddDays(7));

        user.AddRefreshToken(newRefreshToken);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            newAccessToken,
            newRefreshTokenValue,
            newRefreshToken.ExpiresAt,
            new AuthUserResponse(user.Id, user.FullName, user.Email, user.Role.ToString()));
    }
}
