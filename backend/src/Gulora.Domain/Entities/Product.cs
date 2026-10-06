using Gulora.Domain.Common;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;
using Gulora.Domain.ValueObjects;

namespace Gulora.Domain.Entities;

/// <summary>
/// Ürün aggregate root'u.
/// Stok, fiyat, kategori ilişkisi ve görselleri barındırır.
/// Soft delete: <see cref="IsActive"/> false yapılır.
/// EF Core attribute'ları bu sınıfta kesinlikle kullanılmaz — yapılandırma Infrastructure'dadır.
/// </summary>
public sealed class Product : BaseAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ShortDescription { get; private set; }
    public decimal Price { get; private set; }
    public decimal? DiscountedPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public string? Sku { get; private set; }
    public string? Brand { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? SellerId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; }
    public double AverageRating { get; private set; }
    public int ReviewCount { get; private set; }

    private readonly List<ProductImage> _images = [];
    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();

    private readonly List<Review> _reviews = [];
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();

    private Product() { }

    public static Product Create(
        string name,
        string slug,
        decimal price,
        int stockQuantity,
        Guid categoryId,
        string? description = null,
        string? shortDescription = null,
        string? sku = null,
        string? brand = null,
        Guid? sellerId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        if (price < 0)
            throw new BusinessRuleException("Ürün fiyatı negatif olamaz.");

        if (stockQuantity < 0)
            throw new BusinessRuleException("Stok miktarı negatif olamaz.");

        return new Product
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            Description = description?.Trim(),
            ShortDescription = shortDescription?.Trim(),
            Sku = sku?.Trim().ToUpperInvariant(),
            Brand = brand?.Trim(),
            SellerId = sellerId,
            IsActive = true
        };
    }

    public void UpdateDetails(
        string name,
        string slug,
        string? description,
        string? shortDescription,
        string? brand,
        Guid categoryId,
        bool isFeatured)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim();
        ShortDescription = shortDescription?.Trim();
        Brand = brand?.Trim();
        CategoryId = categoryId;
        IsFeatured = isFeatured;
        MarkUpdated();
    }

    /// <summary>Fiyatı günceller. Geçmiş fiyat kaydı Application katmanında tutulabilir.</summary>
    public void UpdatePrice(decimal newPrice, decimal? discountedPrice = null)
    {
        if (newPrice < 0)
            throw new BusinessRuleException("Fiyat negatif olamaz.");

        if (discountedPrice.HasValue && discountedPrice.Value >= newPrice)
            throw new BusinessRuleException("İndirimli fiyat, normal fiyattan küçük olmalıdır.");

        Price = newPrice;
        DiscountedPrice = discountedPrice;
        MarkUpdated();
    }

    /// <summary>Stok azaltır (sipariş oluşturulurken). Yetersiz stokta exception fırlatır.</summary>
    public void DecrementStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Miktar pozitif olmalıdır.");

        if (StockQuantity < quantity)
            throw new BusinessRuleException(
                $"'{Name}' ürünü için yeterli stok yok. Mevcut: {StockQuantity}, İstenen: {quantity}.",
                "INSUFFICIENT_STOCK");

        var oldStock = StockQuantity;
        StockQuantity -= quantity;
        MarkUpdated();

        AddDomainEvent(new ProductStockUpdatedEvent(Id, Name, oldStock, StockQuantity));
    }

    /// <summary>Stok ekler (iade veya manuel artırım).</summary>
    public void IncrementStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Miktar pozitif olmalıdır.");

        var oldStock = StockQuantity;
        StockQuantity += quantity;
        MarkUpdated();

        AddDomainEvent(new ProductStockUpdatedEvent(Id, Name, oldStock, StockQuantity));
    }

    /// <summary>Aktif görsel URL'si veya varsayılan placeholder döner.</summary>
    public string? PrimaryImageUrl =>
        _images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsActive)?.Url;

    /// <summary>Ürünü devre dışı bırakır (soft delete).</summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    /// <summary>Ürünü aktif eder.</summary>
    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    /// <summary>Yeni ürün görseli ekler (maksimum 10).</summary>
    public void AddImage(ProductImage image)
    {
        if (_images.Count >= 10)
            throw new BusinessRuleException("Bir ürüne en fazla 10 görsel eklenebilir.");

        _images.Add(image);
        MarkUpdated();
    }

    /// <summary>Görsel kaldırır.</summary>
    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new NotFoundException(nameof(ProductImage), imageId);

        _images.Remove(image);
        MarkUpdated();
    }

    /// <summary>
    /// Yorum onaylandıktan / silindiğinden sonra ortalama puan ve yorum sayısını günceller.
    /// Application handler'ı <see cref="IReviewRepository"/> üzerinden istatistikleri hesaplar
    /// ve bu metodu çağırır.
    /// </summary>
    public void RecalculateRating(double newAverage, int newCount)
    {
        AverageRating = Math.Round(newAverage, 2);
        ReviewCount = newCount;
        MarkUpdated();
    }

    /// <summary>Geçerli satış fiyatı (indirimli fiyat varsa o, yoksa normal fiyat).</summary>
    public decimal EffectivePrice => DiscountedPrice ?? Price;

    /// <summary>Satışta mı? (aktif ve stok > 0)</summary>
    public bool IsAvailableForPurchase => IsActive && StockQuantity > 0;
}
