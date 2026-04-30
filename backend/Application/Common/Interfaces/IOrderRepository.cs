using Gulora.Domain.Entities;

namespace Gulora.Application.Common.Interfaces;

/// <summary>
/// <see cref="Order"/> toplu kök varlık erişim sözleşmesi. Sipariş listeleme, tekil getir
/// ve yeni sipariş ekleme (oluşturma) senaryoları uygulama use-case'leri tarafından kullanılır.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Tek bir siparişi öğe/izleme (tracking) ayrıntılarıyla yükler; detay ekranları ve admin geçişleri için.</summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Kullanıcının tüm siparişlerini (tarih sıralaması gibi) döner; müşteri "siparişlerim" listesi.</summary>
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Siparişi bağlama ekler; fiyat, satırlar ve alan alanı domain tarafında zaten sağlamlanmıştır.</summary>
    Task AddAsync(Order order, CancellationToken ct = default);

    /// <summary>Adres, durum, stok vb. tüm toplu değişikliklerini kalıcı hale getirir.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
