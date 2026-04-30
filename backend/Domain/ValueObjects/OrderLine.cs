namespace Gulora.Domain.ValueObjects;

/// <summary>
/// Sipariş oluşturma sırasında aggregate dışından gelen satır bilgisi.
/// <c>Order.Create</c> fabrika metodu içinde her satır, doğru sipariş kimliği ile
/// <see cref="Gulora.Domain.Entities.OrderItem"/> örneğine dönüştürülür.
/// </summary>
/// <param name="ProductId">Siparişe eklenecek ürün kimliği.</param>
/// <param name="ProductName">Anlık kopyalanan ürün adı.</param>
/// <param name="ProductImageUrl">Liste / onay ekranı için görsel URL’si.</param>
/// <param name="Quantity">Satın alınan adet (pozitif olmalıdır).</param>
/// <param name="UnitPrice">Sipariş anındaki birim fiyat.</param>
/// <param name="VariantLabel">Renk, boyut vb. seçenek metni; yoksa null.</param>
/// <param name="Addons">Ek kalemler (vazo, mesaj kartı); yoksa null veya boş liste.</param>
public record OrderLine(
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    int Quantity,
    decimal UnitPrice,
    string? VariantLabel = null,
    IReadOnlyList<string>? Addons = null)
{
    /// <summary>Tüm satır alanlarını sipariş öncesi doğrular (tek doğrulama noktası).</summary>
    public void EnsureValid()
    {
        ArgumentOutOfRangeException.ThrowIfEqual(ProductId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(ProductName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ProductImageUrl);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(UnitPrice);
        if (Addons is { Count: > 0 })
        {
            foreach (var a in Addons)
            {
                if (string.IsNullOrWhiteSpace(a))
                    throw new ArgumentException("Əlavə sətirlərdə boş element ola bilməz.", nameof(Addons));
            }
        }
    }
}
