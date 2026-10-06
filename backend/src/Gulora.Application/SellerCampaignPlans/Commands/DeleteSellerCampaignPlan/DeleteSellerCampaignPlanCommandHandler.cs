using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Commands.DeleteSellerCampaignPlan;

public sealed class DeleteSellerCampaignPlanCommandHandler
    : IRequestHandler<DeleteSellerCampaignPlanCommand, Unit>
{
    private readonly ISellerCampaignPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteSellerCampaignPlanCommandHandler(
        ISellerCampaignPlanRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteSellerCampaignPlanCommand cmd, CancellationToken ct)
    {
        var userId = SellerCampaignAuth.RequireSellerUserId(_currentUser);
        var entity = await _repository.GetByIdAndSellerAsync(cmd.Id, userId, ct)
            ?? throw new NotFoundException("Kampaniya planı", cmd.Id);

        _repository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
