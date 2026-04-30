using Gulora.Domain.Common;
using Gulora.Domain.Events;

namespace Gulora.Domain.Entities;

/// <summary>
/// Ürün kataloğundaki kategoriyi temsil eder (ör. çiçekler, hediye paketleri).
/// URL ve filtreleme için <see cref="Slug"/> benzersiz olmalıdır (EF yapılandırmasında indeks).
/// </summary>
public class Category : BaseEntity
{
    /// <summary>Kullanıcıya gösterilen kategori adı.</summary>
    public string Name { get; private set; } = default!;
    /// <summary>URL-dostu benzersiz tanımlayıcı; genelde küçük harf ve tire içerir.</summary>
    public string Slug { get; private set; } = default!;
    /// <summary>UI’da ikon adı veya kodu (isteğe bağlı).</summary>
    public string? Icon { get; private set; }
    /// <summary>Tema rengi hex veya token (isteğe bağlı).</summary>
    public string? Color { get; private set; }
    /// <summary>Kategori kapak görseli URL’si.</summary>
    public string? ImageUrl { get; private set; }
    /// <summary>Listelerde sıralama önceliği; düşük değer önce gösterilebilir.</summary>
    public int SortOrder { get; private set; }

    private Category() { }

    /// <summary>Yeni kategori oluşturur.</summary>
    /// <param name="name">Görünen ad.</param>
    /// <param name="slug">URL parçası; metot içinde küçük harfe çevrilir.</param>
    /// <param name="icon">UI ikon tanımlayıcısı; isteğe bağlı.</param>
    /// <param name="color">Tema rengi; isteğe bağlı.</param>
    public static Category Create(string name, string slug, string? icon = null, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        var category = new Category
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim()
        };
        category.RegisterDomainEvent(new CategoryCreatedDomainEvent(category.Id));
        return category;
    }

    /// <summary>Listələrdə sıra nömrəsini yeniləyir.</summary>
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        SetUpdated();
    }

    /// <summary>Kapak görseli və tema alanlarını yeniləyir.</summary>
    public void UpdatePresentation(string? imageUrl, string? icon, string? color)
    {
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        SetUpdated();
    }

    /// <summary>Yalnız ad və slug (URL) yenilənməsi.</summary>
    public void Rename(string name, string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        SetUpdated();
    }
}
