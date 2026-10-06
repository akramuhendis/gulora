using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = Gulora.Domain.Entities.RefreshToken;

namespace Gulora.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task Handle(ForgotPasswordCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(cmd.Email, ct);

        // Kullanıcı bulunamazsa veya pasifse sessizce geç (bilgi toplama saldırısına karşı).
        if (user is null || !user.IsActive) return;

        var resetTokenValue = _jwtTokenService.GenerateRefreshToken();

        // Sıfırlama token'ı 1 saatliğine geçerli; mevcut oturumlar iptal edilmez.
        var resetToken = RefreshTokenEntity.Create(
            user.Id,
            resetTokenValue,
            DateTime.UtcNow.AddHours(1));

        user.AddPasswordResetToken(resetToken);

        await _unitOfWork.SaveChangesAsync(ct);

        await _emailService.SendPasswordResetAsync(user.Email, user.FullName, resetTokenValue, ct);
    }
}
