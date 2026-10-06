using FluentValidation;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerApplications.Commands.RejectSellerApplication;

public sealed record RejectSellerApplicationCommand(Guid ApplicationId, string Reason) : IRequest;

public sealed class RejectSellerApplicationCommandHandler : IRequestHandler<RejectSellerApplicationCommand>
{
    private readonly ISellerApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectSellerApplicationCommandHandler(
        ISellerApplicationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RejectSellerApplicationCommand cmd, CancellationToken ct)
    {
        var app = await _repository.GetByIdAsync(cmd.ApplicationId, ct)
            ?? throw new NotFoundException("Satıcı başvurusu", cmd.ApplicationId);

        app.Reject(cmd.Reason);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

public sealed class RejectSellerApplicationCommandValidator
    : AbstractValidator<RejectSellerApplicationCommand>
{
    public RejectSellerApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
