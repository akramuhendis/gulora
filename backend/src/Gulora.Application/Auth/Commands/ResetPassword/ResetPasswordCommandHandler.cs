using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using MediatR;

namespace Gulora.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ResetPasswordCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(cmd.Token, ct)
            ?? throw new UnauthorizedException("Parola sıfırlama token'ı geçersiz veya süresi dolmuş.");

        if (!user.IsActive)
            throw new UnauthorizedException("Hesabınız devre dışı bırakılmış.");

        var tokenMatch = user.RefreshTokens.Any(t => t.Token == cmd.Token && t.IsActive);
        if (!tokenMatch)
            throw new UnauthorizedException("Parola sıfırlama token'ı geçersiz veya süresi dolmuş.");

        var newHash = _passwordHasher.Hash(cmd.NewPassword);
        user.ChangePassword(newHash);
        user.RevokeAllRefreshTokens();

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
