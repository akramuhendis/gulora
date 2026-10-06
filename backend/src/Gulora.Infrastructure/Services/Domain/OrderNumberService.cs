using Gulora.Domain.Interfaces;
using Gulora.Domain.Services;
using Gulora.Domain.ValueObjects;

namespace Gulora.Infrastructure.Services.Domain;

/// <summary>
/// Sipariş numarası üretim servisi.
/// Format: "GUL-{YYYYMMDD}-{seq:D4}" — örn. "GUL-20260502-0001".
/// Günlük sıra numarası için veritabanından anlık sayım yapılır (basit strateji).
/// Yüksek yükde atomik sıra için PostgreSQL sequence'e geçilebilir.
/// </summary>
internal sealed class OrderNumberService : IOrderNumberService
{
    private readonly IOrderRepository _orderRepository;

    public OrderNumberService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderNumber> GenerateAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyCount = await _orderRepository.GetDailySequenceAsync(today, ct);
        var sequence = dailyCount + 1;

        return OrderNumber.Generate(today, sequence);
    }
}
