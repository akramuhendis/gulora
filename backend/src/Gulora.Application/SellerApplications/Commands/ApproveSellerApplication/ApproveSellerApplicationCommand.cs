using FluentValidation;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerApplications.Commands.ApproveSellerApplication;

public sealed record ApproveSellerApplicationCommand(Guid ApplicationId) : IRequest;

public sealed class ApproveSellerApplicationCommandHandler : IRequestHandler<ApproveSellerApplicationCommand>
{
    private readonly ISellerApplicationRepository _applicationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveSellerApplicationCommandHandler(
        ISellerApplicationRepository applicationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApproveSellerApplicationCommand cmd, CancellationToken ct)
    {
        var app = await _applicationRepository.GetByIdAsync(cmd.ApplicationId, ct)
            ?? throw new NotFoundException("Satıcı başvurusu", cmd.ApplicationId);

        app.Approve();

        var user = await _userRepository.GetByEmailAsync(app.Email, ct);
        if (user is not null)
            user.PromoteToSeller();

        await _unitOfWork.SaveChangesAsync(ct);
    }
}

public sealed class ApproveSellerApplicationCommandValidator
    : AbstractValidator<ApproveSellerApplicationCommand>
{
    public ApproveSellerApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
    }
}
