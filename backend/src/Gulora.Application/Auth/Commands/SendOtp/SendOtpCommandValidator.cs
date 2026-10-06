using FluentValidation;

namespace Gulora.Application.Auth.Commands.SendOtp;

public sealed class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
    }
}
