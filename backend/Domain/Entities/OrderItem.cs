using Gulora.Domain.Common;

namespace Gulora.Domain.Entities;

/// <summary>
/// Bir sipariş içindeki tek ürün satırı. Sipariş oluşturulduktan sonra ürün fiyatı değişse bile
/// burada kayıtlı <see cref="UnitPrice"/> ve <see cref="ProductName"/> değişmez (anlık kopya).
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>Bağlı <see cref="Order"/> kimliği.</summary>
    public Guid OrderId { get; private set; }
    /// <summary>Sipariş anındaki ürün kimliği (ürün silinse bile geçmiş için saklanır).</summary>
    public Guid ProductId { get; private set; }
    /// <summary>Listelerde gösterim için kopyalanan ürün adı.</summary>
    public string ProductName { get; private set; } = default!;
    /// <summary>Küçük önizleme görseli URL’si.</summary>
    public string ProductImageUrl { get; private set; } = default!;
    /// <summary>Satın alınan adet.</summary>
    public int Quantity { get; private set; }
    /// <summary>Birim fiyat (vergi dahil/haric iş kuralına göre uygulama katmanında netleştirilir).</summary>
    public decimal UnitPrice { get; private set; }
    /// <summary>Renk, boyut gibi varyant etiketi (varsa).</summary>
    public string? VariantLabel { get; private set; }
    private readonly List<string> _addons = [];
    /// <summary>Ek ürün veya hizmet adları (ör. vazo, kart mesajı).</summary>
    public IReadOnlyCollection<string> Addons => _addons;

    private OrderItem() { }

    /// <summary>Satır oluşturur; genelde yalnızca <see cref="Order.Create"/> içinden çağrılır.</summary>
    /// <param name="orderId">Bağlı sipariş kimliği.</param>
    /// <param name="productId">Ürün kimliği.</param>
    /// <param name="productName">Anlık kopyalanan ürün adı; boş olamaz.</param>
    /// <param name="productImageUrl">Görsel URL’si; boş olamaz.</param>
    /// <param name="quantity">Adet; sıfırdan büyük olmalıdır.</param>
    /// <param name="unitPrice">Birim fiyat; negatif olamaz.</param>
    /// <param name="variantLabel">Varyant etiketi; yoksa null.</param>
    /// <param name="addons">Ek kalemler; boş veya null ise boş liste atanır, aksi halde kopya alınır.</param>
    public static OrderItem Create(
        Guid orderId, Guid productId,
        string productName, string productImageUrl,
        int quantity, decimal unitPrice,
        string? variantLabel = null,
        IReadOnlyList<string>? addons = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(orderId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(productImageUrl);

        var item = new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName.Trim(),
            ProductImageUrl = productImageUrl.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            VariantLabel = variantLabel?.Trim(),
        };
        if (addons is { Count: > 0 })
            item._addons.AddRange(addons.Where(static a => !string.IsNullOrWhiteSpace(a)).Select(static a => a.Trim()));
        return item;
    }

    /// <summary>Birim fiyat × adet (ek ücretler ayrı iş kuralı ile eklenebilir).</summary>
    public decimal LineTotal => UnitPrice * Quantity;
}
