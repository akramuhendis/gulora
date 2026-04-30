using Gulora.Domain.Common;
using Gulora.Domain.Events;
using Gulora.Domain.Exceptions;

namespace Gulora.Domain.Entities;

/// <summary>
/// Satışa sunulan tek bir ürünü temsil eder. Fiyat, stok ve görsel bilgileri burada tutulur;
/// sipariş anında satır kalemi (<see cref="OrderItem"/>) üzerinde anlık kopya saklanır.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>Ürün listesinde ve detayda gösterilen ad.</summary>
    public string Name { get; private set; } = default!;
    /// <summary>Ürün detay URL’si için benzersiz slug.</summary>
    public string Slug { get; private set; } = default!;
    /// <summary>Güncel satış fiyatı (pozitif veya sıfır).</summary>
    public decimal Price { get; private set; }
    /// <summary>İndirim öncesi eski fiyat; kampanya gösterimi için (isteğe bağlı).</summary>
    public decimal? OriginalPrice { get; private set; }
    /// <summary>Yüzde indirim etiketi için; isteğe bağlı.</summary>
    public int? DiscountPercent { get; private set; }
    /// <summary>Ortalama yıldız puanı (genelde 0–5 aralığında tutulur).</summary>
    public decimal Rating { get; private set; }
    /// <summary>Değerlendirme sayısı; yeni yorum eklendikçe güncellenir.</summary>
    public int ReviewCount { get; private set; }
    /// <summary>Ana liste görseli URL’si.</summary>
    public string ImageUrl { get; private set; } = default!;
    private readonly List<string> _images = [];
    /// <summary>Galeri görselleri; EF tarafında JSON veya ayrı tablo ile eşlenebilir.</summary>
    public IReadOnlyCollection<string> Images => _images;
    /// <summary>Bağlı olduğu <see cref="Category"/> kimliği.</summary>
    public Guid CategoryId { get; private set; }
    private readonly List<string> _tags = [];
    /// <summary>Arama ve filtreleme için etiketler.</summary>
    public IReadOnlyCollection<string> Tags => _tags;
    /// <summary>Örn. "Yeni", "Çok Satan" gibi rozet metni.</summary>
    public string? Badge { get; private set; }
    /// <summary>Aynı gün teslimat seçeneğine uygun ürün mü.</summary>
    public bool IsSameDay { get; private set; }
    /// <summary>Bu ürün için kargo ücreti muafiyeti.</summary>
    public bool IsFreeDelivery { get; private set; }
    /// <summary>HTML veya düz metin açıklama.</summary>
    public string? Description { get; private set; }
    /// <summary>Stok adedi; sipariş onayında düşürülür.</summary>
    public int Stock { get; private set; }
    /// <summary>Katalogdan kaldırılmış ürünler false olabilir.</summary>
    public bool IsActive { get; private set; } = true;

    private Product() { }

    /// <summary>Minimum alanlarla yeni ürün oluşturur; admin panelinden ek alanlar sonra eklenebilir.</summary>
    public static Product Create(
        string name, string slug, decimal price,
        string imageUrl, Guid categoryId, int stock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stock);
        ArgumentOutOfRangeException.ThrowIfEqual(categoryId, Guid.Empty);

        var product = new Product
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Price = price,
            ImageUrl = imageUrl.Trim(),
            CategoryId = categoryId,
            Stock = stock
        };
        product.RegisterDomainEvent(new ProductCreatedDomainEvent(product.Id));
        return product;
    }

    /// <summary>Ad, slug ve kategori kimliğini günceller.</summary>
    public void UpdateIdentity(string name, string slug, Guid categoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentOutOfRangeException.ThrowIfEqual(categoryId, Guid.Empty);
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        CategoryId = categoryId;
        SetUpdated();
    }

    /// <summary>Satış fiyatını günceller; köhnə qiymət (original) cari fiyattan kiçik qala bilməz.</summary>
    public void UpdatePrice(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        if (OriginalPrice.HasValue && OriginalPrice.Value < price)
            throw new DomainException("Köhnə qiymət cari qiymətdən kiçik ola bilməz.");
        Price = price;
        SetUpdated();
    }

    /// <summary>Ana görsel URL’si.</summary>
    public void UpdatePrimaryImage(string imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);
        ImageUrl = imageUrl.Trim();
        SetUpdated();
    }

    /// <summary>İndirim vitrin alanları (faiz etiketi və keçmiş qiymət).</summary>
    public void UpdatePromotion(decimal? originalPrice, int? discountPercent)
    {
        if (discountPercent is < 0 or > 100)
            throw new DomainException("Endirim faizi 0–100 aralığında olmalıdır.");
        if (originalPrice.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(originalPrice.Value);
            if (originalPrice.Value < Price)
                throw new DomainException("Köhnə qiymət cari qiymətdən kiçik ola bilməz.");
        }

        OriginalPrice = originalPrice;
        DiscountPercent = discountPercent;
        SetUpdated();
    }

    public void SetBadge(string? badge) { Badge = string.IsNullOrWhiteSpace(badge) ? null : badge.Trim(); SetUpdated(); }

    public void SetSameDayDelivery(bool value) { IsSameDay = value; SetUpdated(); }

    public void SetFreeDelivery(bool value) { IsFreeDelivery = value; SetUpdated(); }

    public void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SetUpdated();
    }

    public void ActivateCatalog()
    {
        if (IsActive) return;
        IsActive = true;
        SetUpdated();
    }

    public void DeactivateCatalog()
    {
        if (!IsActive) return;
        IsActive = false;
        SetUpdated();
    }

    /// <summary>Admin və ya senkronizasyon; galeri URL listesini değiştirir (mevcut liste yerine geçer).</summary>
    public void ReplaceImages(IEnumerable<string> urls)
    {
        _images.Clear();
        _images.AddRange(urls.Where(static u => !string.IsNullOrWhiteSpace(u)).Select(static u => u.Trim()));
        SetUpdated();
    }

    /// <summary>Etiket listesini günceller.</summary>
    public void ReplaceTags(IEnumerable<string> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags.Where(static t => !string.IsNullOrWhiteSpace(t)).Select(static t => t.Trim()));
        SetUpdated();
    }

    /// <summary>Sipariş və ya rezervasyon sonrası stoktan düşer.</summary>
    public void DecreaseStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (Stock < quantity)
            throw new DomainException($"'{Name}' məhsulu üçün kifayət qədər stok yoxdur.");
        Stock -= quantity;
        SetUpdated();
    }

    /// <summary>İptal, iade və ya inventar düzəlişi üçün stoku artırır.</summary>
    public void IncreaseStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        Stock += quantity;
        SetUpdated();
    }

    /// <summary>Bütün yorumlar yeniden hesaplandığında ortalama puan güncellenir.</summary>
    public void UpdateRating(decimal newRating, int newCount)
    {
        if (newRating is < 0 or > 5)
            throw new DomainException("Reytinq 0 ilə 5 arasında olmalıdır.");
        ArgumentOutOfRangeException.ThrowIfNegative(newCount);
        Rating = newRating;
        ReviewCount = newCount;
        SetUpdated();
    }
}
