using Gulora.Application.Common.Extensions;
using Gulora.Contracts.Orders;
using Gulora.Contracts.Seller;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Seller.Queries.GetSellerDashboard;

public sealed class GetSellerDashboardQueryHandler
    : IRequestHandler<GetSellerDashboardQuery, SellerDashboardResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public GetSellerDashboardQueryHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<SellerDashboardResponse> Handle(
        GetSellerDashboardQuery query, CancellationToken ct)
    {
        var threshold = Math.Clamp(query.LowStockThreshold, 1, 999);
        var trendDays = Math.Clamp(query.TrendDayCount, 1, 90);
        var recentTake = Math.Clamp(query.RecentOrderTake, 1, 50);

        var orderSnapshot = await _orderRepository.GetSellerDashboardOrderSnapshotAsync(
            query.SellerUserId,
            recentTake,
            trendDays,
            ct);

        var totalProducts = await _productRepository.CountBySellerAsync(query.SellerUserId, ct);
        var (lowStock, outOfStock) = await _productRepository.CountSellerStockBucketsAsync(
            query.SellerUserId,
            threshold,
            ct);

        var attributed = await _orderRepository.GetSellerAttributedTotalsByOrderIdsAsync(
            orderSnapshot.RecentOrders.Select(o => o.Id).ToList(),
            query.SellerUserId,
            ct);

        var endUtc = DateTime.UtcNow.Date;
        var buckets = new List<SellerDashboardDayBucketResponse>(trendDays);
        for (var i = trendDays - 1; i >= 0; i--)
        {
            var d = DateOnly.FromDateTime(endUtc.AddDays(-i));
            var count = orderSnapshot.OrdersPerDayUtc.TryGetValue(d, out var c) ? c : 0;
            var sellerTotal = orderSnapshot.SellerLinesPerDayUtc.TryGetValue(d, out var total)
                ? total
                : 0m;
            buckets.Add(new SellerDashboardDayBucketResponse(d.ToString("yyyy-MM-dd"), count, sellerTotal));
        }

        var statusBreakdown = orderSnapshot.StatusCounts
            .Select(t => new SellerDashboardStatusCountResponse(
                t.Status.ToString(),
                t.Status.ToDisplayString(),
                t.Count))
            .ToList();

        var recentItems = orderSnapshot.RecentOrders.Select(o =>
        {
            var sellerTotal = attributed.TryGetValue(o.Id, out var at) ? at : 0m;
            var primary = o.Items.FirstOrDefault()?.ProductName;

            return new OrderSummaryResponse(
                o.Id,
                o.OrderNumber,
                o.Status.ToString(),
                o.Status.ToDisplayString(),
                o.DeliveryType.ToString(),
                o.PaymentMethod.ToString(),
                o.PaymentStatus.ToString(),
                o.TotalAmount,
                o.Items.Count,
                primary,
                null,
                o.CanBeCancelled,
                o.CanBeReturned,
                o.CreatedAt,
                sellerTotal);
        }).ToList();

        return new SellerDashboardResponse(
            totalProducts,
            lowStock,
            outOfStock,
            threshold,
            orderSnapshot.TotalOrderCount,
            orderSnapshot.PendingFulfillmentCount,
            statusBreakdown,
            orderSnapshot.GmvSellerLines,
            orderSnapshot.RealizedSellerLines,
            buckets,
            recentItems);
    }
}
