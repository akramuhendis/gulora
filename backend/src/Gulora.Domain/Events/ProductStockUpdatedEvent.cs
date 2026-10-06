using Gulora.Domain.Common;

namespace Gulora.Domain.Events;

/// <summary>
/// Ürün stok miktarı değiştiğinde yayınlanır (satış, iade, manuel güncelleme).
/// Handler'lar: düşük stok uyarısı, stok tükendi bildirimi.
/// </summary>
public sealed record ProductStockUpdatedEvent(
    Guid ProductId,
    string ProductName,
    int OldStock,
    int NewStock
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>Stok kritik eşiğin altına düştü mü? (eşik: 5)</summary>
    public bool IsLowStock => NewStock is > 0 and <= 5;

    /// <summary>Stok tamamen tükendi mi?</summary>
    public bool IsOutOfStock => NewStock == 0;
}
