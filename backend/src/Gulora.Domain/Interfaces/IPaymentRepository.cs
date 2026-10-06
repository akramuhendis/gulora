using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

/// <summary>
/// Ödeme kaydı sorgu operasyonları.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>İdempotency anahtarıyla ödeme getirir (çift ödeme önleme).</summary>
    Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default);

    /// <summary>Siparişe ait ödeme kayıtlarını getirir.</summary>
    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
