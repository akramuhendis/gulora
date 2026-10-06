using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Auth;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using MediatR;
using RefreshTokenEntity = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(cmd.Email, ct)
            ?? throw new UnauthorizedException("E-posta veya parola hatalı.");

        if (!user.IsActive)
            throw new UnauthorizedException("Hesabınız devre dışı bırakılmış.");

        if (!_passwordHasher.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedException("E-posta veya parola hatalı.");

        user.RecordLogin();

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, DateTime.UtcNow.AddDays(7));

        user.AddRefreshToken(refreshToken);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            refreshToken.ExpiresAt,
            new AuthUserResponse(user.Id, user.FullName, user.Email, user.Role.ToString()));
    }
}
