using AutoMapper;
using Gulora.Application.SellerCampaignPlans;
using Gulora.Contracts.Products;
using Gulora.Domain.Enums;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProductBySlug;

public sealed class GetProductBySlugQueryHandler : IRequestHandler<GetProductBySlugQuery, ProductDetailResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISellerShopRepository _sellerShopRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISellerCampaignPlanRepository _sellerCampaignPlanRepository;
    private readonly IMapper _mapper;

    public GetProductBySlugQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISellerShopRepository sellerShopRepository,
        IUserRepository userRepository,
        ISellerCampaignPlanRepository sellerCampaignPlanRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _sellerShopRepository = sellerShopRepository;
        _userRepository = userRepository;
        _sellerCampaignPlanRepository = sellerCampaignPlanRepository;
        _mapper = mapper;
    }

    public async Task<ProductDetailResponse> Handle(GetProductBySlugQuery query, CancellationToken ct)
    {
        var slug = query.Slug.Trim();
        var product = await _productRepository.GetBySlugAsync(slug, ct)
            ?? throw new NotFoundException($"'{slug}' slug'ına sahip ürün bulunamadı.");

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, ct);

        var response = _mapper.Map<ProductDetailResponse>(product) with
        {
            CategoryName = category?.Name ?? string.Empty
        };

        SellerShopSummaryResponse? sellerShop = null;
        if (product.SellerId is Guid sellerUserId)
        {
            var sellerUser = await _userRepository.GetByIdAsync(sellerUserId, ct);
            var shop = await _sellerShopRepository.GetByUserIdAsync(sellerUserId, ct);
            if (sellerUser is not null
                && sellerUser.Role == UserRole.Seller
                && sellerUser.IsActive
                && shop is not null)
            {
                sellerShop = new SellerShopSummaryResponse(
                    sellerUserId,
                    shop.StoreName,
                    shop.StoreSlug,
                    shop.LogoUrl);
            }
        }

        ProductSellerCampaignOffer? sellerCampaign = null;
        if (product.SellerId is Guid campaignSellerId)
        {
            var plans = await _sellerCampaignPlanRepository.ListActiveBySellerAsync(
                campaignSellerId,
                DateTime.UtcNow,
                ct);
            sellerCampaign = SellerCampaignBestDiscountResolver.ResolveForProduct(plans, product);
        }

        return response with { SellerShop = sellerShop, SellerCampaign = sellerCampaign };
    }
}
