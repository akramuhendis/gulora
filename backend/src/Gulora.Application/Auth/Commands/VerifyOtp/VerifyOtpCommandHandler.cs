using System.Collections.Generic;
using Gulora.Contracts.Auth;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;
using MediatR;

namespace Gulora.Application.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, OtpResponse>
{
    private readonly IPhoneOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public VerifyOtpCommandHandler(
        IPhoneOtpRepository otpRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _otpRepository = otpRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<OtpResponse> Handle(VerifyOtpCommand cmd, CancellationToken ct)
    {
        PhoneNumber phoneVo;
        try
        {
            phoneVo = PhoneNumber.Of(cmd.PhoneNumber);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                new Dictionary<string, string[]> { ["PhoneNumber"] = [ex.Message] });
        }

        var phone = phoneVo.Value;
        var challenge = await _otpRepository.GetLatestByPhoneAsync(phone, ct)
            ?? throw new UnauthorizedException("Geçerli bir doğrulama kodu bulunamadı.");

        if (challenge.IsExpired(DateTime.UtcNow))
            throw new UnauthorizedException("Doğrulama kodunun süresi dolmuş.");

        if (!_passwordHasher.Verify(cmd.Code.Trim(), challenge.CodeHash))
            throw new UnauthorizedException("Doğrulama kodu hatalı.");

        await _otpRepository.DeleteByPhoneAsync(phone, ct);

        if (cmd.LinkUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(cmd.LinkUserId.Value, ct)
                ?? throw new NotFoundException("Kullanıcı", cmd.LinkUserId.Value);

            if (!user.IsActive)
                throw new UnauthorizedException("Hesabınız devre dışı bırakılmış.");

            user.VerifyPhoneNumber(phone);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return new OtpResponse(
            IsVerified: true,
            Message: "Telefon numarası doğrulandı.",
            ExpiresAt: null);
    }
}
