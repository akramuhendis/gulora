using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Commands.UpdateSellerCampaignPlan;

public sealed class UpdateSellerCampaignPlanCommandHandler
    : IRequestHandler<UpdateSellerCampaignPlanCommand, SellerCampaignPlanResponse>
{
    private readonly ISellerCampaignPlanRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateSellerCampaignPlanCommandHandler(
        ISellerCampaignPlanRepository repository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SellerCampaignPlanResponse> Handle(UpdateSellerCampaignPlanCommand cmd, CancellationToken ct)
    {
        var userId = SellerCampaignAuth.RequireSellerUserId(_currentUser);
        var entity = await _repository.GetByIdAndSellerAsync(cmd.Id, userId, ct)
            ?? throw new NotFoundException("Kampaniya planı", cmd.Id);

        var starts = DateTimeUtc.AsUtc(cmd.StartsAt);
        var ends = DateTimeUtc.AsUtc(cmd.EndsAt);

        var scopeJson = await CampaignScopeNormalizer.NormalizeAsync(
            cmd.Scope,
            userId,
            _categoryRepository,
            _productRepository,
            ct);

        entity.Update(
            cmd.Name,
            cmd.Kind,
            cmd.Percent,
            starts,
            ends,
            cmd.MinCartAmount,
            scopeJson,
            cmd.Notes);

        await _unitOfWork.SaveChangesAsync(ct);

        return SellerCampaignPlanMapper.ToResponse(entity);
    }
}
