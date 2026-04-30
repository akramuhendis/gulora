using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Users.Commands.ChangePassword;

/// <summary>
/// Oturum açmış kullanıcının mevcut parolasını doğrulayarak yeni parolaya günceller.
/// </summary>
public sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher) : IRequestHandler<ChangePasswordCommand, Unit>
{
    /// <summary>
    /// Kullanıcıyı yükler, mevcut parolayı doğrular, yeni parolayı hashler ve kalıcı olarak kaydeder.
    /// </summary>
    /// <param name="request">Kullanıcı kimliği, mevcut ve yeni parola.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>İşlem tamamlandığında MediatR birim değeri.</returns>
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Mevcut parola hash ile eşleşmiyorsa işlemi güvenlik nedeniyle reddet.
        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("The current password is incorrect.");

        var newHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newHash);
        await users.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
