using FluentValidation;

namespace Gulora.Application.Notifications.Commands.MarkNotificationsRead;

public sealed class MarkNotificationsReadCommandValidator : AbstractValidator<MarkNotificationsReadCommand>
{
    public MarkNotificationsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.NotificationIds)
            .NotNull().WithMessage("Bildirim kimlik listesi null olamaz.");
    }
}
