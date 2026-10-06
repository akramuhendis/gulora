using Gulora.Contracts.Seller;
using MediatR;

namespace Gulora.Application.Seller.Queries.GetSellerDashboard;

public sealed record GetSellerDashboardQuery(
    Guid SellerUserId,
    int RecentOrderTake = 8,
    int TrendDayCount = 14,
    int LowStockThreshold = 10) : IRequest<SellerDashboardResponse>;
