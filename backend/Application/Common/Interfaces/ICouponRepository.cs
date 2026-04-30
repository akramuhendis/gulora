using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// <see cref="Coupon"/> (indirim kuponu) kodu ile sorgu ve kupon kullanım metriklerinin kalıcılığı.
/// Siparişte indirim ve validate endpoint’i bu sözleşmeyi kullanır.
/// </summary>
public interface ICouponRepository
{
    /// <summary>Normalize büyük harf kodu ile kupon; bulunamadığında <c>null</c>.</summary>
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>İndirim, kullanım artışı vb. tüm toplu değişiklikleri yazar.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
