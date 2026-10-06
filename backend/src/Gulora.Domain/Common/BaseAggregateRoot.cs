namespace Gulora.Domain.Common;

/// <summary>
/// DDD Aggregate Root için özelleştirilmiş temel sınıf.
/// <see cref="BaseEntity"/> üzerine inşa edilmiştir; tutarlılık sınırını işaret eder.
/// Aggregate root dışındaki entity'ler doğrudan repository üzerinden erişilemez —
/// her zaman kök üzerinden ulaşılmalıdır.
/// </summary>
public abstract class BaseAggregateRoot : BaseEntity
{
    // Gelecekte aggregate-seviye kilitler, optimistic concurrency (RowVersion) veya
    // domain policy hook'ları buraya eklenebilir.
}
