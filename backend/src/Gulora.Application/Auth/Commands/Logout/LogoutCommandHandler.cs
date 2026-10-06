using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(cmd.RefreshToken, ct);
        if (user is null) return;

        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == cmd.RefreshToken && t.IsActive);
        token?.Revoke();

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
