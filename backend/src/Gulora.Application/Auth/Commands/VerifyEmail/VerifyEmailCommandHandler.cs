using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(VerifyEmailCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(cmd.Token.Trim(), ct)
            ?? throw new Gulora.Domain.Exceptions.UnauthorizedException(
                "E-posta doğrulama bağlantısı geçersiz.");

        user.CompleteEmailVerification(cmd.Token.Trim());

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
