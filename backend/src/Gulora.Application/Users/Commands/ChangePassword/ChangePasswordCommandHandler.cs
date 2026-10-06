using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using MediatR;

namespace Gulora.Application.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ChangePasswordCommand cmd, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(cmd.UserId, ct)
            ?? throw new NotFoundException("Kullanıcı", cmd.UserId);

        if (!_passwordHasher.Verify(cmd.CurrentPassword, user.PasswordHash))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["CurrentPassword"] = ["Mevcut parola hatalı."]
            });

        var newHash = _passwordHasher.Hash(cmd.NewPassword);
        user.ChangePassword(newHash);
        user.RevokeAllRefreshTokens();

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
