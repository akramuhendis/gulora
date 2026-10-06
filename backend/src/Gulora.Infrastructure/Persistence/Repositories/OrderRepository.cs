using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(GuloraDbContext context) : base(context) { }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => await Context.Orders
            .Include(o => o.Items)
            .Include(o => o.Trackings)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<IReadOnlyList<Order>> GetByUserIdPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
        => await Context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Context.Orders.CountAsync(o => o.UserId == userId, ct);

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken ct = default)
        => await Context.Orders
            .Where(o => o.Status == status)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
        => await Context.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task<int> GetDailySequenceAsync(DateOnly date, CancellationToken ct = default)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await Context.Orders
            .CountAsync(o => o.CreatedAt >= startOfDay && o.CreatedAt <= endOfDay, ct);
    }

    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Context.Orders
            .Include(o => o.Items)
            .Include(o => o.Trackings)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<bool> OrderInvolvesSellerAsync(
        Guid orderId,
        Guid sellerUserId,
        CancellationToken ct = default)
        => await (
            from o in Context.Orders
            join i in Context.OrderItems on o.Id equals i.OrderId
            join p in Context.Products.IgnoreQueryFilters() on i.ProductId equals p.Id
            where o.Id == orderId && p.SellerId == sellerUserId
            select o.Id).AnyAsync(ct);

    public async Task<bool> OrderIsFullyOwnedBySellerAsync(
        Guid orderId,
        Guid sellerUserId,
        CancellationToken ct = default)
    {
        var sellerIds = await (
            from i in Context.OrderItems
            join p in Context.Products.IgnoreQueryFilters() on i.ProductId equals p.Id
            where i.OrderId == orderId
            select p.SellerId).ToListAsync(ct);

        return sellerIds.Count > 0
               && sellerIds.All(sid => sid == sellerUserId);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedForSellerAsync(
        Guid sellerUserId,
        OrderStatus? status,
        string? orderNumber,
        DateTime? dateFrom,
        DateTime? dateTo,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = OrdersForSeller(sellerUserId);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var pattern = $"%{orderNumber.Trim()}%";
            query = query.Where(o => EF.Functions.ILike(o.OrderNumber, pattern));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetSellerAttributedTotalsByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        Guid sellerUserId,
        CancellationToken ct = default)
    {
        if (orderIds.Count == 0)
            return new Dictionary<Guid, decimal>();

        var rows = await (
            from i in Context.OrderItems
            join p in Context.Products.IgnoreQueryFilters() on i.ProductId equals p.Id
            where orderIds.Contains(i.OrderId) && p.SellerId == sellerUserId
            group i by i.OrderId
            into g
            select new { OrderId = g.Key, Total = g.Sum(x => x.UnitPrice * x.Quantity) }
        ).ToListAsync(ct);

        return rows.ToDictionary(x => x.OrderId, x => x.Total);
    }

    public async Task<SellerDashboardOrderSnapshot> GetSellerDashboardOrderSnapshotAsync(
        Guid sellerUserId,
        int recentOrderTake,
        int trendDayCount,
        CancellationToken ct = default)
    {
        trendDayCount = Math.Clamp(trendDayCount, 1, 90);
        recentOrderTake = Math.Clamp(recentOrderTake, 1, 50);

        var baseQ = OrdersForSeller(sellerUserId);

        var totalOrderCount = await baseQ.CountAsync(ct);

        var pendingFulfillmentCount = await baseQ.CountAsync(
            o => o.Status == OrderStatus.Pending
                 || o.Status == OrderStatus.Confirmed
                 || o.Status == OrderStatus.Processing,
            ct);

        var statusRows = await baseQ
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var statusCounts = statusRows
            .Select(r => (r.Status, r.Count))
            .OrderByDescending(x => x.Count)
            .ToList();

        var gmvSellerLines = await (
            from o in Context.Orders
            from i in o.Items
            join p in Context.Products.IgnoreQueryFilters() on i.ProductId equals p.Id
            where p.SellerId == sellerUserId
                  && o.Status != OrderStatus.Cancelled
                  && o.Status != OrderStatus.Returned
                  && o.Status != OrderStatus.ReturnRequested
            select i.UnitPrice * i.Quantity).SumAsync(ct);

        var realizedSellerLines = await (
            from o in Context.Orders
            from i in o.Items
            join p in Context.Products.IgnoreQueryFilters() on i.ProductId equals p.Id
            where p.SellerId == sellerUserId
                  && (o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
            select i.UnitPrice * i.Quantity).SumAsync(ct);

        var endUtc = DateTime.UtcNow.Date;
        var startUtc = endUtc.AddDays(-(trendDayCount - 1));

        var dayRows = await (
            from o in Context.Orders
            where o.Items.Any(i =>
                Context.Products.IgnoreQueryFilters().Any(p =>
                    p.Id == i.ProductId && p.SellerId == sellerUserId))
                  && o.CreatedAt >= startUtc
                  && o.CreatedAt < endUtc.AddDays(1)
            group o by o.CreatedAt.Date
            into g
            select new { Day = DateOnly.FromDateTime(g.Key), Count = g.Count() }).ToListAsync(ct);

        var ordersPerDay = dayRows.ToDictionary(x => x.Day, x => x.Count);

        var dayTotalRows = await (
            from o in Context.Orders
            from i in o.Items
            join p in Context.Products.IgnoreQueryFilters() on i.ProductId equals p.Id
            where p.SellerId == sellerUserId
                  && o.CreatedAt >= startUtc
                  && o.CreatedAt < endUtc.AddDays(1)
                  && o.Status != OrderStatus.Cancelled
                  && o.Status != OrderStatus.Returned
                  && o.Status != OrderStatus.ReturnRequested
            group i by o.CreatedAt.Date
            into g
            select new { Day = DateOnly.FromDateTime(g.Key), Total = g.Sum(x => x.UnitPrice * x.Quantity) })
            .ToListAsync(ct);

        var sellerLinesPerDay = dayTotalRows.ToDictionary(x => x.Day, x => x.Total);

        var recentOrders = await baseQ
            .OrderByDescending(o => o.CreatedAt)
            .Take(recentOrderTake)
            .Include(o => o.Items)
            .ToListAsync(ct);

        return new SellerDashboardOrderSnapshot(
            totalOrderCount,
            pendingFulfillmentCount,
            statusCounts,
            gmvSellerLines,
            realizedSellerLines,
            ordersPerDay,
            sellerLinesPerDay,
            recentOrders);
    }

    private IQueryable<Order> OrdersForSeller(Guid sellerUserId)
        => Context.Orders.Where(o => o.Items.Any(i =>
            Context.Products.IgnoreQueryFilters().Any(p =>
                p.Id == i.ProductId && p.SellerId == sellerUserId)));
}
