using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.SellerCampaigns;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.SellerCampaignPlans.Commands.CreateSellerCampaignPlan;

public sealed class CreateSellerCampaignPlanCommandHandler
    : IRequestHandler<CreateSellerCampaignPlanCommand, SellerCampaignPlanResponse>
{
    private readonly ISellerCampaignPlanRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateSellerCampaignPlanCommandHandler(
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

    public async Task<SellerCampaignPlanResponse> Handle(CreateSellerCampaignPlanCommand cmd, CancellationToken ct)
    {
        var userId = SellerCampaignAuth.RequireSellerUserId(_currentUser);
        var starts = DateTimeUtc.AsUtc(cmd.StartsAt);
        var ends = DateTimeUtc.AsUtc(cmd.EndsAt);

        var scopeJson = await CampaignScopeNormalizer.NormalizeAsync(
            cmd.Scope,
            userId,
            _categoryRepository,
            _productRepository,
            ct);

        var entity = SellerCampaignPlan.Create(
            userId,
            cmd.Name,
            cmd.Kind,
            cmd.Percent,
            starts,
            ends,
            cmd.MinCartAmount,
            scopeJson,
            cmd.Notes);

        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return SellerCampaignPlanMapper.ToResponse(entity);
    }
}
