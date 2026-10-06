using System.Collections.Generic;
using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Auth;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;
using MediatR;

namespace Gulora.Application.Auth.Commands.SendOtp;

public sealed class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, OtpResponse>
{
    private const int CooldownSeconds = 45;
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);

    private readonly IPhoneOtpRepository _otpRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISmsService _smsService;

    public SendOtpCommandHandler(
        IPhoneOtpRepository otpRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ISmsService smsService)
    {
        _otpRepository = otpRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _smsService = smsService;
    }

    public async Task<OtpResponse> Handle(SendOtpCommand cmd, CancellationToken ct)
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
        var utcNow = DateTime.UtcNow;

        var existing = await _otpRepository.GetLatestByPhoneAsync(phone, ct);
        if (existing is not null && (utcNow - existing.SentAt).TotalSeconds < CooldownSeconds)
        {
            throw new TooManyRequestsException(
                $"Yeni bir kod istemeden önce {CooldownSeconds} saniye bekleyin.");
        }

        var plainCode = GenerateSixDigitCode();
        var hash = _passwordHasher.Hash(plainCode);

        await _otpRepository.DeleteByPhoneAsync(phone, ct);

        var challenge = PhoneOtpChallenge.Create(phone, hash, OtpLifetime);
        _otpRepository.Add(challenge);

        await _unitOfWork.SaveChangesAsync(ct);

        await _smsService.SendOtpAsync(phone, plainCode, ct);

        return new OtpResponse(
            IsVerified: false,
            Message: "Doğrulama kodu gönderildi.",
            ExpiresAt: challenge.ExpiresAt);
    }

    private static string GenerateSixDigitCode()
    {
        var n = Random.Shared.Next(0, 1_000_000);
        return n.ToString("D6");
    }
}
