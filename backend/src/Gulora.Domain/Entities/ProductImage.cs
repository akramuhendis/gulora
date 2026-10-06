using Gulora.Domain.Common;

namespace Gulora.Domain.Entities;

/// <summary>
/// Ürüne ait görsel kaydı.
/// Product aggregate'inin alt entity'sidir.
/// </summary>
public sealed class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string url, string? altText = null, int displayOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new ProductImage
        {
            ProductId = productId,
            Url = url.Trim(),
            AltText = altText?.Trim(),
            DisplayOrder = displayOrder,
            IsActive = true
        };
    }

    public void UpdateOrder(int newOrder)
    {
        DisplayOrder = newOrder;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
