using Gulora.Contracts.Orders;

namespace Gulora.Contracts.Seller;

/// <summary>GET api/v1/seller/dashboard — satıcı icmal göstəriciləri (server aqreqatları).</summary>
public sealed record SellerDashboardResponse(
    int TotalProductCount,
    int LowStockCount,
    int OutOfStockCount,
    int LowStockThreshold,
    int TotalOrderCount,
    int PendingFulfillmentCount,
    IReadOnlyList<SellerDashboardStatusCountResponse> StatusBreakdown,
    decimal GmvSellerAttributed,
    decimal RealizedSellerAttributed,
    IReadOnlyList<SellerDashboardDayBucketResponse> OrdersLastDays,
    IReadOnlyList<OrderSummaryResponse> RecentOrders);

public sealed record SellerDashboardStatusCountResponse(
    string Status,
    string StatusLabel,
    int Count);

public sealed record SellerDashboardDayBucketResponse(
    /// <summary>UTC günü yyyy-MM-dd.</summary>
    string Date,
    int OrderCount,
    decimal SellerAttributedTotal);
