using Gulora.Domain.Common;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Sipariş kalemi — Order aggregate'inin alt entity'sidir.
/// Ürün adı ve fiyatı sipariş anında anlık olarak kaydedilir (ProductSnapshot);
/// ürün sonradan değişse bile sipariş verisi bozulmaz.
/// </summary>
public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    /// <summary>Sipariş anındaki ürün adı (snapshot).</summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>Sipariş anındaki SKU (snapshot).</summary>
    public string? ProductSku { get; private set; }

    /// <summary>Sipariş anındaki birim fiyat (snapshot).</summary>
    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>Kalem toplamı = UnitPrice × Quantity.</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem() { }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity,
        string? productSku = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        if (unitPrice < 0)
            throw new BusinessRuleException("Birim fiyat negatif olamaz.");

        if (quantity <= 0)
            throw new BusinessRuleException("Kalem miktarı en az 1 olmalıdır.");

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName.Trim(),
            ProductSku = productSku?.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
