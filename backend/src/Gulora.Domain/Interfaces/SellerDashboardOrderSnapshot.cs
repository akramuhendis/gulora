using Gulora.Domain.Entities;
using Gulora.Domain.Enums;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Satıcı paneli icmalı üçün sifariş tərəfi aqreqatları (repository çıxışı).
/// </summary>
public sealed record SellerDashboardOrderSnapshot(
    int TotalOrderCount,
    int PendingFulfillmentCount,
    IReadOnlyList<(OrderStatus Status, int Count)> StatusCounts,
    decimal GmvSellerLines,
    decimal RealizedSellerLines,
    IReadOnlyDictionary<DateOnly, int> OrdersPerDayUtc,
    IReadOnlyDictionary<DateOnly, decimal> SellerLinesPerDayUtc,
    IReadOnlyList<Order> RecentOrders);
