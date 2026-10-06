using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Services;

/// <summary>
/// Benzersiz sipariş numarası üretiminden sorumlu domain servis arayüzü.
/// Uygulama Infrastructure katmanındadır (DB sıra numarası veya başka strateji).
/// </summary>
public interface IOrderNumberService
{
    /// <summary>
    /// Bugünün tarihine ve sıra numarasına göre yeni bir sipariş numarası üretir.
    /// Format: "GUL-{YYYYMMDD}-{seq:D4}" — örn. "GUL-20260502-0001".
    /// Thread-safe olmalıdır.
    /// </summary>
    Task<OrderNumber> GenerateAsync(CancellationToken ct = default);
}
