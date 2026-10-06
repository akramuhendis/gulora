using Gulora.Application.Common.Interfaces;
using Gulora.Application.Common.Security;
using Gulora.Contracts.Auth;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using MediatR;
using RefreshTokenEntity = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        if (await _userRepository.IsEmailTakenAsync(cmd.Email, ct))
            throw new ConflictException("Bu e-posta adresi zaten kullanılıyor.");

        var passwordHash = _passwordHasher.Hash(cmd.Password);
        var user = User.Create(cmd.FirstName, cmd.LastName, cmd.Email, passwordHash);

        if (cmd.PhoneNumber is not null)
            user.UpdateProfile(user.FirstName, user.LastName, cmd.PhoneNumber);

        _userRepository.Add(user);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, DateTime.UtcNow.AddDays(7));

        user.AddRefreshToken(refreshToken);

        var emailVerificationToken = SecureToken.CreateHexToken();
        user.SetPendingEmailVerification(emailVerificationToken, DateTime.UtcNow.AddHours(24));

        await _unitOfWork.SaveChangesAsync(ct);

        await _emailService.SendEmailVerificationAsync(
            user.Email, user.FullName, emailVerificationToken, ct);

        return new AuthResponse(
            accessToken,
            refreshTokenValue,
            refreshToken.ExpiresAt,
            new AuthUserResponse(user.Id, user.FullName, user.Email, user.Role.ToString()));
    }
}
