using AutoMapper;
using Gulora.Contracts.Orders;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponse> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId, ct)
            ?? throw new NotFoundException("Sipariş", query.OrderId);

        if (query.IsAdmin)
        {
            // tam erişim
        }
        else if (query.SellerScopeUserId.HasValue)
        {
            if (!await _orderRepository.OrderInvolvesSellerAsync(
                    query.OrderId,
                    query.SellerScopeUserId.Value,
                    ct))
                throw new ForbiddenException("Bu siparişi görüntüleme yetkiniz yok.");
        }
        else if (order.UserId != query.RequestedByUserId)
            throw new ForbiddenException("Bu siparişi görüntüleme yetkiniz yok.");

        var response = _mapper.Map<OrderResponse>(order);
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var imageUrls = await _productRepository.GetPrimaryImageUrlsByProductIdsAsync(productIds, ct);

        var enrichedItems = response.Items
            .Select(i => i with
            {
                ProductImageUrl = imageUrls.TryGetValue(i.ProductId, out var url) ? url : null
            })
            .ToList();

        return response with { Items = enrichedItems };
    }
}
